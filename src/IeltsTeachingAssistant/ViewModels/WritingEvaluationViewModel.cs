using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;

namespace IeltsTeachingAssistant.ViewModels;

public partial class WritingEvaluationViewModel : ObservableObject
{
    private readonly IVertexAIService _vertexAIService;
    private readonly IEvaluationService _evaluationService;
    private readonly AppDbContext _dbContext;
    private readonly Dictionary<int, (double TR, double CC, double LR, double GRA)> _writingAiBackups = new();

    private readonly System.Threading.SemaphoreSlim _saveSemaphore = new(1, 1);
    private int _loadingRefCount;

    private void IncrementLoading()
    {
        System.Threading.Interlocked.Increment(ref _loadingRefCount);
        IsLoading = true;
    }

    private void DecrementLoading()
    {
        if (System.Threading.Interlocked.Decrement(ref _loadingRefCount) <= 0)
        {
            _loadingRefCount = 0;
            IsLoading = false;
        }
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        _writingAiBackups.Clear();
        if (value != null)
        {
            WritingTasks.Clear();
            WritingTasks.Add(new WritingTask { TaskNumber = 1, TaskType = "Academic" });
            WritingTasks.Add(new WritingTask { TaskNumber = 2, TaskType = "Academic" });
            SelectedTask = WritingTasks.FirstOrDefault();
        }
    }

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    public string[] EvaluationModes { get; } = new[] { "Standard AI", "Detailed Feedback", "Exam Mode" };

    [ObservableProperty]
    private string _selectedEvaluationMode = "Standard AI";

    [ObservableProperty]
    private ObservableCollection<WritingTask> _writingTasks = new();

    [ObservableProperty]
    private List<MatrixCriterion> _matrixDescriptors = new();

    [ObservableProperty]
    private WritingTask? _selectedTask;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isDirty;

    public WritingEvaluationViewModel(
        IVertexAIService vertexAIService,
        IEvaluationService evaluationService,
        AppDbContext dbContext)
    {
        _vertexAIService = vertexAIService ?? throw new ArgumentNullException(nameof(vertexAIService));
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        InitializeMatrixDescriptors();
        LoadStudents();
        ClearSession();
    }

    public void LoadStudents()
    {
        Students.Clear();
        foreach (var student in _dbContext.Students.ToList())
        {
            Students.Add(student);
        }
    }

    private void InitializeMatrixDescriptors()
    {
        MatrixDescriptors = IeltsDescriptors.WritingDescriptors.Select(cd => new MatrixCriterion
        {
            CriterionKey = cd.CriterionKey,
            CriterionName = cd.CriterionName,
            Bands = cd.Bands.Select(bd => new MatrixBand
            {
                Band = bd.Band,
                Points = bd.Points.Select(pd =>
                {
                    var point = new MatrixPoint
                    {
                        Id = pd.Id,
                        Text = pd.Text,
                        IsSelected = false
                    };
                    point.PropertyChanged += Point_PropertyChanged;
                    return point;
                }).ToList()
            }).ToList()
        }).ToList();
    }

    private void Point_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MatrixPoint.IsSelected) && sender is MatrixPoint changedPoint)
        {
            IsDirty = true;
            if (changedPoint.IsSelected)
            {
                foreach (var criterion in MatrixDescriptors)
                {
                    var bandWithPoint = criterion.Bands.FirstOrDefault(b => b.Points.Any(p => p.Id == changedPoint.Id));
                    if (bandWithPoint != null)
                    {
                        // Deselect other points in the same criterion
                        foreach (var b in criterion.Bands)
                        {
                            foreach (var p in b.Points)
                            {
                                if (p != changedPoint)
                                {
                                    p.PropertyChanged -= Point_PropertyChanged;
                                    p.IsSelected = false;
                                    p.PropertyChanged += Point_PropertyChanged;
                                }
                            }
                        }

                        // Update individual band score on the active WritingTask
                        if (SelectedTask != null)
                        {
                            UpdateTaskScore(SelectedTask, criterion.CriterionKey, bandWithPoint.Band);
                            OnPropertyChanged(nameof(SelectedTask));
                        }
                        break;
                    }
                }
            }
        }
    }

    private void UpdateTaskScore(WritingTask task, string key, double score)
    {
        switch (key)
        {
            case "TR":
                task.TaskAchievement = score;
                break;
            case "CC":
                task.CoherenceCohesion = score;
                break;
            case "LR":
                task.LexicalResource = score;
                break;
            case "GRA":
                task.GrammaticalRange = score;
                break;
        }
    }

    public void LoadDescriptorsFromTask(WritingTask task)
    {
        if (task == null || string.IsNullOrEmpty(IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(task))) return;
        try
        {
            var ids = JsonSerializer.Deserialize<List<string>>(IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(task));
            if (ids != null)
            {
                foreach (var criterion in MatrixDescriptors)
                {
                    foreach (var band in criterion.Bands)
                    {
                        foreach (var point in band.Points)
                        {
                            point.PropertyChanged -= Point_PropertyChanged;
                            point.IsSelected = ids.Contains(point.Id);
                            point.PropertyChanged += Point_PropertyChanged;
                        }
                    }
                }
            }
        }
        catch { }
        BuildRubricHighlights(task);
    }

    /// <summary>
    /// Populates the per-criterion <see cref="WritingTask.RubricHighlightIds0"/> –
    /// <see cref="WritingTask.RubricHighlightIds3"/> properties from the AI-matched
    /// descriptor IDs stored on the task. Called after AI grading and after manual
    /// descriptor load so the <see cref="Controls.RubricGridPanel"/> always reflects
    /// the current state.
    /// </summary>
    public void BuildRubricHighlights(WritingTask task)
    {
        if (task == null) return;

        var raw = IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(task);
        List<string>? allIds = null;
        if (!string.IsNullOrEmpty(raw))
        {
            try { allIds = JsonSerializer.Deserialize<List<string>>(raw); } catch { }
        }
        allIds ??= new List<string>();

        var descriptors = IeltsDescriptors.WritingDescriptors;
        var highlightProps = new System.Action<IReadOnlyList<string>?>[]{
            v => task.RubricHighlightIds0 = v,
            v => task.RubricHighlightIds1 = v,
            v => task.RubricHighlightIds2 = v,
            v => task.RubricHighlightIds3 = v,
        };

        for (int i = 0; i < descriptors.Count && i < highlightProps.Length; i++)
        {
            var criterion = descriptors[i];
            var criterionIds = new List<string>();
            foreach (var band in criterion.Bands)
                foreach (var point in band.Points)
                    if (allIds.Contains(point.Id))
                        criterionIds.Add(point.Id);
            highlightProps[i](criterionIds.Count > 0 ? criterionIds : null);
        }
    }

    [RelayCommand]
    public async Task GradeWithAiAsync(WritingTask task)
    {
        IncrementLoading();
        ErrorMessage = null;
        IsErrorVisible = false;
        try
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(task.Prompt) || string.IsNullOrWhiteSpace(task.SubmissionText))
            {
                throw new ArgumentException("Prompt and submission text cannot be empty.");
            }

            var result = await _vertexAIService.GradeWritingAsync(
                task.Prompt,
                task.SubmissionText,
                task.TaskNumber,
                SelectedEvaluationMode,
                task.TaskType ?? "Academic"
            );

            task.TaskAchievement = result.AnalyticalCriteriaScores.TaskResponse.Band;
            task.CoherenceCohesion = result.AnalyticalCriteriaScores.CoherenceCohesion.Band;
            task.LexicalResource = result.AnalyticalCriteriaScores.LexicalResource.Band;
            task.GrammaticalRange = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.Band;

            task.TaskAchievementAIComment = result.AnalyticalCriteriaScores.TaskResponse.KeyJustification;
            task.CoherenceCohesionAIComment = result.AnalyticalCriteriaScores.CoherenceCohesion.KeyJustification;
            task.LexicalResourceAIComment = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            task.GrammaticalRangeAIComment = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;

            task.TaskAchievementJustification = result.AnalyticalCriteriaScores.TaskResponse.KeyJustification;
            task.TaskAchievementEvidence = string.Join("; ", result.AnalyticalCriteriaScores.TaskResponse.SupportingEvidenceQuotes);
            task.TaskAchievementLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.TaskResponse.LimitingFactors);

            task.CoherenceCohesionJustification = result.AnalyticalCriteriaScores.CoherenceCohesion.KeyJustification;
            task.CoherenceCohesionEvidence = string.Join("; ", result.AnalyticalCriteriaScores.CoherenceCohesion.SupportingEvidenceQuotes);
            task.CoherenceCohesionLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.CoherenceCohesion.LimitingFactors);

            task.LexicalResourceJustification = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            task.LexicalResourceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.SupportingEvidenceQuotes);
            task.LexicalResourceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.LimitingFactors);

            task.GrammaticalRangeJustification = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            task.GrammaticalRangeEvidence = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.SupportingEvidenceQuotes);
            task.GrammaticalRangeLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.LimitingFactors);

            task.CoreStrengths = result.StudentCoaching.CoreStrengths;
            task.PrimaryWeakness = result.StudentCoaching.PrimaryWeaknessToFix;
            task.ActionablePractice = result.StudentCoaching.ActionablePracticeExercise;

            var allMatchedIds = new List<string>();
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.TaskResponse.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.CoherenceCohesion.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.LexicalResource.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.MatchedDescriptorIds);

            IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.SetSelectedRubricDescriptors(task, JsonSerializer.Serialize(allMatchedIds));
            
            // Backup the AI scores
            _writingAiBackups[task.TaskNumber] = (
                task.TaskAchievement,
                task.CoherenceCohesion,
                task.LexicalResource,
                task.GrammaticalRange
            );

            LoadDescriptorsFromTask(task);
            IsDirty = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorVisible = true;
        }
        finally
        {
            DecrementLoading();
        }
    }

    public void RevertToAiScore(WritingTask task)
    {
        if (task == null) return;
        if (_writingAiBackups.TryGetValue(task.TaskNumber, out var scores))
        {
            task.TaskAchievement = scores.TR;
            task.CoherenceCohesion = scores.CC;
            task.LexicalResource = scores.LR;
            task.GrammaticalRange = scores.GRA;
            
            LoadDescriptorsFromTask(task);
            IsDirty = true;
        }
    }

    public string? CheckForConflicts(WritingTask task)
    {
        if (task == null) return null;
        if (_writingAiBackups.TryGetValue(task.TaskNumber, out var scores))
        {
            if (Math.Abs(task.TaskAchievement - scores.TR) >= 2.0 ||
                Math.Abs(task.CoherenceCohesion - scores.CC) >= 2.0 ||
                Math.Abs(task.LexicalResource - scores.LR) >= 2.0 ||
                Math.Abs(task.GrammaticalRange - scores.GRA) >= 2.0)
            {
                return "Conflict detected: manual score differs from AI score by >= 2.0 bands.";
            }
        }
        return null;
    }

    [RelayCommand]
    public async Task SaveSessionAsync()
    {
        if (!_saveSemaphore.Wait(0))
        {
            return;
        }
        IncrementLoading();
        ErrorMessage = null;
        IsErrorVisible = false;
        try
        {
            if (SelectedStudent == null)
            {
                throw new InvalidOperationException("No student selected.");
            }
            var eval = new WritingEvaluation
            {
                StudentId = SelectedStudent.Id,
                ClassId = SelectedStudent.ClassId,
                EvaluationMode = SelectedEvaluationMode,
                TestType = TestType.Academic,
                EvaluatedAt = DateTime.UtcNow
            };
            foreach (var task in WritingTasks)
            {
                eval.Tasks.Add(task);
            }
            await _evaluationService.CreateWritingEvaluationAsync(eval);
            IsDirty = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorVisible = true;
        }
        finally
        {
            DecrementLoading();
            _saveSemaphore.Release();
        }
    }

    [RelayCommand]
    public void ClearSession()
    {
        SelectedStudent = null;
        SelectedEvaluationMode = "Standard AI";
        WritingTasks.Clear();
        WritingTasks.Add(new WritingTask { TaskNumber = 1, TaskType = "Academic" });
        WritingTasks.Add(new WritingTask { TaskNumber = 2, TaskType = "Academic" });
        SelectedTask = WritingTasks.FirstOrDefault();

        foreach (var criterion in MatrixDescriptors)
        {
            foreach (var band in criterion.Bands)
            {
                foreach (var point in band.Points)
                {
                    point.PropertyChanged -= Point_PropertyChanged;
                    point.IsSelected = false;
                    point.PropertyChanged += Point_PropertyChanged;
                }
            }
        }
        _writingAiBackups.Clear();
        IsDirty = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    public void GoToNext()
    {
        if (WritingTasks.Count == 0) return;
        if (SelectedTask == null)
        {
            SelectedTask = WritingTasks.FirstOrDefault();
        }
        else
        {
            int index = WritingTasks.IndexOf(SelectedTask);
            if (index >= 0 && index < WritingTasks.Count - 1)
            {
                SelectedTask = WritingTasks[index + 1];
            }
        }
    }

    public bool CheckDirtyWarning()
    {
        return IsDirty;
    }

    [ObservableProperty]
    private bool _isFocusMode;

    [ObservableProperty]
    private int _activePivotIndex;

    [ObservableProperty]
    private bool _isErrorVisible;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Controls.InfoBarSeverity _infoBarSeverity;

    public Microsoft.UI.Xaml.Visibility SimultaneousModeVisibility => IsFocusMode ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility FocusModeVisibility => IsFocusMode ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    partial void OnIsFocusModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SimultaneousModeVisibility));
        OnPropertyChanged(nameof(FocusModeVisibility));
    }

    public bool HasUnsavedChanges
    {
        get => IsDirty;
        set => IsDirty = value;
    }

    private WritingEvaluation _evaluation = new();
    public WritingEvaluation Evaluation 
    {
        get
        {
            _evaluation.Tasks = WritingTasks;
            return _evaluation;
        }
    }

    public double EvaluationOverallBand => Evaluation.OverallBand;
    public double EvaluationTaskAchievement => Evaluation.TaskAchievement;
    public double EvaluationCoherenceCohesion => Evaluation.CoherenceCohesion;
    public double EvaluationLexicalResource => Evaluation.LexicalResource;
    public double EvaluationGrammaticalRange => Evaluation.GrammaticalRange;

    public Task InitializeAsync() => Task.CompletedTask;

    public void UpdateAverages()
    {
        OnPropertyChanged(nameof(Evaluation));
        OnPropertyChanged(nameof(EvaluationOverallBand));
        OnPropertyChanged(nameof(EvaluationTaskAchievement));
        OnPropertyChanged(nameof(EvaluationCoherenceCohesion));
        OnPropertyChanged(nameof(EvaluationLexicalResource));
        OnPropertyChanged(nameof(EvaluationGrammaticalRange));
    }

    public async Task LoadDocumentContentAsync(WritingTask task, string filePath)
    {
        if (task == null || string.IsNullOrEmpty(filePath)) return;
        try
        {
            if (filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                using (var archive = new System.IO.Compression.ZipArchive(fileStream))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry == null)
                    {
                        throw new System.IO.InvalidDataException("Invalid Word document: main content (word/document.xml) is missing.");
                    }
                    using (var entryStream = entry.Open())
                    {
                        var doc = System.Xml.Linq.XDocument.Load(entryStream);
                        var w = (System.Xml.Linq.XNamespace)"http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                        var paragraphs = doc.Descendants(w + "p");
                        var paragraphTexts = new List<string>();
                        foreach (var p in paragraphs)
                        {
                            var pText = string.Concat(p.Descendants().Select(el => {
                                if (el.Name == w + "t") return el.Value;
                                if (el.Name == w + "br") return Environment.NewLine;
                                if (el.Name == w + "tab") return "\t";
                                return "";
                            }));
                            if (!string.IsNullOrEmpty(pText))
                            {
                                paragraphTexts.Add(pText);
                            }
                        }
                        task.SubmissionText = string.Join(Environment.NewLine, paragraphTexts);
                    }
                }
                task.OriginalFilePath = filePath;
                return;
            }

            if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                task.SubmissionText = await System.IO.File.ReadAllTextAsync(filePath);
            }
            else
            {
                task.SubmissionText = await _vertexAIService.ExtractTextFromPdfOrImageAsync(filePath);
            }
            task.OriginalFilePath = filePath;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load document: {ex.Message}";
            IsErrorVisible = true;
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
        }
    }
}


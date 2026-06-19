using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using IeltsTeachingAssistant.Data;
using Microsoft.EntityFrameworkCore;

namespace IeltsTeachingAssistant.ViewModels;

public partial class WritingEvaluationViewModel : ObservableObject
{
    private readonly IVertexAIService _vertexAiService;
    private readonly IEvaluationService _evaluationService;
    private readonly AppDbContext _context;

    [ObservableProperty]
    private WritingEvaluation _evaluation;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private int _activePivotIndex = 0;

    public List<string> EvaluationModes { get; } = new()
    {
        "Full Test (Tasks 1 & 2)",
        "Task 1 Only",
        "Task 2 Only"
    };

    [ObservableProperty]
    private string _selectedEvaluationMode = "Full Test (Tasks 1 & 2)";

    partial void OnSelectedEvaluationModeChanged(string value)
    {
        UpdateEvaluationTasks();
    }

    private void UpdateEvaluationTasks()
    {
        var student = SelectedStudent;
        var newEval = new WritingEvaluation
        {
            Student = student,
            StudentId = student?.Id ?? 0,
            EvaluationMode = SelectedEvaluationMode
        };

        if (SelectedEvaluationMode == "Full Test (Tasks 1 & 2)")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = newEval });
            newEval.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Task 1 Only")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Task 2 Only")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = newEval });
        }

        Evaluation = newEval;
        ActivePivotIndex = 0;
        UpdateAverages();
    }

    partial void OnSelectedStudentChanged(Student? oldValue, Student? newValue)
    {
        ClearSessionInternal(newValue);
    }

    private bool _isFocusMode = false;
    public bool IsFocusMode
    {
        get => _isFocusMode;
        set
        {
            if (SetProperty(ref _isFocusMode, value))
            {
                OnPropertyChanged(nameof(SimultaneousModeVisibility));
                OnPropertyChanged(nameof(FocusModeVisibility));
            }
        }
    }

    public Microsoft.UI.Xaml.Visibility SimultaneousModeVisibility => _isFocusMode ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility FocusModeVisibility => _isFocusMode ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    public WritingEvaluationViewModel(
        IVertexAIService vertexAiService,
        IEvaluationService evaluationService,
        AppDbContext context)
    {
        _vertexAiService = vertexAiService;
        _evaluationService = evaluationService;
        _context = context;

        _evaluation = new WritingEvaluation { EvaluationMode = SelectedEvaluationMode };
        _evaluation.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = _evaluation });
        _evaluation.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = _evaluation });
    }

    private void ClearSessionInternal(Student? student)
    {
        var newEval = new WritingEvaluation
        {
            Student = student,
            StudentId = student?.Id ?? 0,
            EvaluationMode = SelectedEvaluationMode
        };

        if (SelectedEvaluationMode == "Full Test (Tasks 1 & 2)")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = newEval });
            newEval.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Task 1 Only")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Task 2 Only")
        {
            newEval.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = newEval });
        }

        Evaluation = newEval;
        ActivePivotIndex = 0;
        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        HasUnsavedChanges = false;
    }

    [RelayCommand]
    public void ClearSession()
    {
        SelectedStudent = null;
        ClearSessionInternal(null);
    }

    public async Task InitializeAsync()
    {
        Students.Clear();
        var students = await _context.Students.ToListAsync();
        foreach (var s in students)
        {
            Students.Add(s);
        }
    }

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isErrorVisible = false;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Controls.InfoBarSeverity _infoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;

    [ObservableProperty]
    private bool _isGrading = false;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    [RelayCommand]
    private async Task GradeWithAiAsync(WritingTask task)
    {
        if (task == null) return;

        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        IsGrading = true;
        InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;

        try
        {
            var taskTypeStr = task.TaskNumber == 1 ? (task.TaskType ?? "Graph") : "Essay";

            var result = await _vertexAiService.GradeWritingAsync(task.Prompt ?? "", task.SubmissionText ?? "", task.TaskNumber, "Academic", taskTypeStr);

            task.TaskAchievement = result.AnalyticalCriteriaScores.TaskResponse.Band;
            task.TaskAchievementAIComment = result.AnalyticalCriteriaScores.TaskResponse.KeyJustification;
            task.TaskAchievementJustification = result.AnalyticalCriteriaScores.TaskResponse.KeyJustification;
            task.TaskAchievementEvidence = string.Join("; ", result.AnalyticalCriteriaScores.TaskResponse.SupportingEvidenceQuotes);
            task.TaskAchievementLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.TaskResponse.LimitingFactors);

            task.CoherenceCohesion = result.AnalyticalCriteriaScores.CoherenceCohesion.Band;
            task.CoherenceCohesionAIComment = result.AnalyticalCriteriaScores.CoherenceCohesion.KeyJustification;
            task.CoherenceCohesionJustification = result.AnalyticalCriteriaScores.CoherenceCohesion.KeyJustification;
            task.CoherenceCohesionEvidence = string.Join("; ", result.AnalyticalCriteriaScores.CoherenceCohesion.SupportingEvidenceQuotes);
            task.CoherenceCohesionLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.CoherenceCohesion.LimitingFactors);

            task.LexicalResource = result.AnalyticalCriteriaScores.LexicalResource.Band;
            task.LexicalResourceAIComment = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            task.LexicalResourceJustification = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            task.LexicalResourceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.SupportingEvidenceQuotes);
            task.LexicalResourceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.LimitingFactors);

            task.GrammaticalRange = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.Band;
            task.GrammaticalRangeAIComment = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            task.GrammaticalRangeJustification = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            task.GrammaticalRangeEvidence = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.SupportingEvidenceQuotes);
            task.GrammaticalRangeLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.LimitingFactors);

            task.CoreStrengths = result.StudentCoaching.CoreStrengths;
            task.PrimaryWeakness = result.StudentCoaching.PrimaryWeaknessToFix;
            task.ActionablePractice = result.StudentCoaching.ActionablePracticeExercise;

            UpdateAverages();
            await SaveSessionAsync();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorVisible = true;
        }
        finally
        {
            IsGrading = false;
        }
    }

    public double EvaluationOverallBand => Evaluation?.OverallBand ?? 0.0;
    public double EvaluationTaskAchievement => Evaluation?.TaskAchievement ?? 0.0;
    public double EvaluationCoherenceCohesion => Evaluation?.CoherenceCohesion ?? 0.0;
    public double EvaluationLexicalResource => Evaluation?.LexicalResource ?? 0.0;
    public double EvaluationGrammaticalRange => Evaluation?.GrammaticalRange ?? 0.0;

    partial void OnEvaluationChanged(WritingEvaluation value)
    {
        UpdateAverages();
    }

    public void UpdateAverages()
    {
        OnPropertyChanged(nameof(EvaluationOverallBand));
        OnPropertyChanged(nameof(EvaluationTaskAchievement));
        OnPropertyChanged(nameof(EvaluationCoherenceCohesion));
        OnPropertyChanged(nameof(EvaluationLexicalResource));
        OnPropertyChanged(nameof(EvaluationGrammaticalRange));
    }

    [RelayCommand]
    public async Task GoToNextAsync()
    {
        if (IsFocusMode)
        {
            if (ActivePivotIndex == 0 && Evaluation.Tasks.Count > 1)
            {
                var task2 = Evaluation.Tasks.FirstOrDefault(t => t.TaskNumber == 2);
                if (task2 != null)
                {
                    task2.SubmissionText = string.Empty;
                }
                ActivePivotIndex = 1;
            }
            else
            {
                await SaveSessionAndSelectNextStudentAsync();
            }
        }
        else
        {
            await SaveSessionAndSelectNextStudentAsync();
        }
    }

    private async Task SaveSessionAndSelectNextStudentAsync()
    {
        if (Evaluation.Student != null && HasUnsavedChanges)
        {
            await SaveSessionAsync();
        }

        if (SelectedStudent != null && Students.Count > 1)
        {
            var currentIndex = Students.IndexOf(SelectedStudent);
            var nextIndex = (currentIndex + 1) % Students.Count;
            var nextStudent = Students[nextIndex];

            var oldPrompts = Evaluation.Tasks.ToDictionary(t => t.TaskNumber, t => (t.Prompt, t.TaskType));

            SelectedStudent = nextStudent;

            foreach (var task in Evaluation.Tasks)
            {
                if (oldPrompts.TryGetValue(task.TaskNumber, out var value))
                {
                    task.Prompt = value.Prompt;
                    task.TaskType = value.TaskType;
                }
            }

            ActivePivotIndex = 0;

            ErrorMessage = $"Switched to student: {nextStudent.Name}. Work fields cleared, prompts preserved.";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
            IsErrorVisible = true;
        }
        else
        {
            ErrorMessage = "All students completed or only one student available.";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
            IsErrorVisible = true;
        }
    }

    public async Task LoadDocumentContentAsync(WritingTask task, string filePath)
    {
        if (task == null || string.IsNullOrEmpty(filePath)) return;

        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        IsGrading = true;

        try
        {
            string ext = Path.GetExtension(filePath).ToLower();
            string content = "";

            if (ext == ".txt")
            {
                content = await File.ReadAllTextAsync(filePath);
            }
            else if (ext == ".docx")
            {
                content = ExtractTextFromDocx(filePath);
            }
            else if (ext == ".pdf" || ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                content = await _vertexAiService.ExtractTextFromPdfOrImageAsync(filePath);
            }
            else
            {
                throw new NotSupportedException("Unsupported file type. Please upload a TXT, DOCX, PDF, or image file.");
            }

            task.SubmissionText = content;
            HasUnsavedChanges = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load document: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
        finally
        {
            IsGrading = false;
        }
    }

    private string ExtractTextFromDocx(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        using var archive = new ZipArchive(fileStream);
        var entry = archive.GetEntry("word/document.xml");
        if (entry == null) return string.Empty;

        using var entryStream = entry.Open();
        var doc = XDocument.Load(entryStream);

        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        var paragraphs = doc.Descendants(w + "p")
            .Select(p => string.Join("", p.Descendants(w + "t").Select(t => t.Value)));

        return string.Join(Environment.NewLine, paragraphs);
    }

    [RelayCommand]
    public async Task SaveSessionAsync()
    {
        if (Evaluation.Student == null)
        {
            ErrorMessage = "Please select a student before saving.";
            IsErrorVisible = true;
            return;
        }

        try
        {
            Evaluation.ClassId = Evaluation.Student.ClassId;
            Evaluation.StudentId = Evaluation.Student.Id;
            Evaluation.EvaluationMode = SelectedEvaluationMode;

            _context.Entry(Evaluation.Student).State = EntityState.Unchanged;
            if (Evaluation.Class != null)
            {
                _context.Entry(Evaluation.Class).State = EntityState.Unchanged;
            }

            if (Evaluation.Id == 0)
            {
                _context.WritingEvaluations.Add(Evaluation);
            }
            else
            {
                var entry = _context.Entry(Evaluation);
                if (entry.State == EntityState.Detached)
                {
                    _context.WritingEvaluations.Update(Evaluation);
                }
            }

            await _context.SaveChangesAsync();

            HasUnsavedChanges = false;
            ErrorMessage = "Session saved successfully!";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
            IsErrorVisible = true;
        }
        catch (System.Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null) msg += "\nInner: " + ex.InnerException.Message;
            ErrorMessage = $"Failed to save: {msg}";
            IsErrorVisible = true;
        }
    }
}

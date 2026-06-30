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

public partial class SpeakingEvaluationViewModel : ObservableObject
{
    private readonly IVertexAIService _vertexAIService;
    private readonly IEvaluationService _evaluationService;
    private readonly IAudioService _audioService;
    private readonly AppDbContext _dbContext;

    private readonly Dictionary<int, (double FC, double LR, double GRA, double PR)> _speakingAiBackups = new();

    private readonly System.Threading.SemaphoreSlim _saveSemaphore = new(1, 1);
    private int _loadingRefCount;
    private SpeakingPart? _currentlyPlayingPart;

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
        _speakingAiBackups.Clear();
        if (value != null)
        {
            SpeakingParts.Clear();
            SpeakingParts.Add(new SpeakingPart { PartNumber = 1 });
            SpeakingParts.Add(new SpeakingPart { PartNumber = 2 });
            SpeakingParts.Add(new SpeakingPart { PartNumber = 3 });
            SelectedPart = SpeakingParts.FirstOrDefault();
            UpdateVisibleSpeakingParts();
        }
    }

    private void AudioService_PlaybackStopped(object? sender, EventArgs e)
    {
        if (_currentlyPlayingPart != null)
        {
            _currentlyPlayingPart.IsPlaying = false;
            _currentlyPlayingPart = null;
        }
    }

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    public string[] EvaluationModes { get; } = new[] { "Standard AI", "Detailed Feedback", "Exam Mode" };

    [ObservableProperty]
    private string _selectedEvaluationMode = "Standard AI";

    public string[] EvaluationScopes { get; } = new[] { "Whole Test (Parts 1-3)", "Part 1", "Part 2", "Part 3" };

    [ObservableProperty]
    private string _selectedEvaluationScope = "Whole Test (Parts 1-3)";

    public ObservableCollection<SpeakingPart> VisibleSpeakingParts { get; } = new();

    partial void OnSelectedEvaluationScopeChanged(string value)
    {
        UpdateVisibleSpeakingParts();
    }

    private void UpdateVisibleSpeakingParts()
    {
        VisibleSpeakingParts.Clear();
        if (SelectedEvaluationScope == "Whole Test (Parts 1-3)")
        {
            foreach (var part in SpeakingParts)
            {
                VisibleSpeakingParts.Add(part);
            }
        }
        else if (SelectedEvaluationScope == "Part 1" && SpeakingParts.Count > 0)
        {
            VisibleSpeakingParts.Add(SpeakingParts[0]);
        }
        else if (SelectedEvaluationScope == "Part 2" && SpeakingParts.Count > 1)
        {
            VisibleSpeakingParts.Add(SpeakingParts[1]);
        }
        else if (SelectedEvaluationScope == "Part 3" && SpeakingParts.Count > 2)
        {
            VisibleSpeakingParts.Add(SpeakingParts[2]);
        }
    }

    [ObservableProperty]
    private ObservableCollection<SpeakingPart> _speakingParts = new();

    [ObservableProperty]
    private List<MatrixCriterion> _matrixDescriptors = new();

    [ObservableProperty]
    private SpeakingPart? _selectedPart;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isDirty;

    public SpeakingEvaluationViewModel(
        IVertexAIService vertexAIService,
        IEvaluationService evaluationService,
        IAudioService audioService,
        AppDbContext dbContext)
    {
        _vertexAIService = vertexAIService ?? throw new ArgumentNullException(nameof(vertexAIService));
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        _audioService.PlaybackStopped += AudioService_PlaybackStopped;

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
        MatrixDescriptors = IeltsDescriptors.SpeakingDescriptors.Select(cd => new MatrixCriterion
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

                        // Update individual band score on the active SpeakingPart
                        if (SelectedPart != null)
                        {
                            UpdatePartScore(SelectedPart, criterion.CriterionKey, bandWithPoint.Band);
                            OnPropertyChanged(nameof(SelectedPart));
                        }
                        break;
                    }
                }
            }
        }
    }

    private void UpdatePartScore(SpeakingPart part, string key, double score)
    {
        switch (key)
        {
            case "FC":
                part.FluencyCoherence = score;
                break;
            case "LR":
                part.LexicalResource = score;
                break;
            case "GRA":
                part.GrammaticalRange = score;
                break;
            case "PR":
                part.Pronunciation = score;
                break;
        }
    }

    public void LoadDescriptorsFromPart(SpeakingPart part)
    {
        if (part == null || string.IsNullOrEmpty(IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(part))) return;
        try
        {
            var ids = JsonSerializer.Deserialize<List<string>>(IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(part));
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
        BuildRubricHighlights(part);
    }

    /// <summary>
    /// Populates the per-criterion RubricHighlightIds0-3 on a <see cref="SpeakingPart"/>
    /// from AI-matched descriptor IDs, so RubricGridPanel highlights the right points.
    /// </summary>
    public void BuildRubricHighlights(SpeakingPart part)
    {
        if (part == null) return;

        var raw = IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.GetSelectedRubricDescriptors(part);
        List<string>? allIds = null;
        if (!string.IsNullOrEmpty(raw))
        {
            try { allIds = JsonSerializer.Deserialize<List<string>>(raw); } catch { }
        }
        allIds ??= new List<string>();

        var descriptors = IeltsDescriptors.SpeakingDescriptors;
        var highlightProps = new System.Action<IReadOnlyList<string>?>[]{
            v => part.RubricHighlightIds0 = v,
            v => part.RubricHighlightIds1 = v,
            v => part.RubricHighlightIds2 = v,
            v => part.RubricHighlightIds3 = v,
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
    public async Task StartRecordingAsync(SpeakingPart part)
    {
        if (part == null) return;
        part.IsRecording = true;
        string uniquePath = $"temp_audio_part_{part.PartNumber}_{Guid.NewGuid():N}.wav";
        await _audioService.StartRecordingAsync(uniquePath);
    }

    [RelayCommand]
    public async Task StopRecordingAsync(SpeakingPart part)
    {
        if (part == null) return;
        string path = _audioService.StopRecording();
        part.IsRecording = false;
        part.AudioFilePath = path;
    }

    [RelayCommand]
    public async Task UploadAudioAsync(SpeakingPart part)
    {
        if (part == null) return;
        part.AudioFilePath = "mock_uploaded_audio.wav";
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task PlayAudioAsync(SpeakingPart part)
    {
        if (part == null || string.IsNullOrEmpty(part.AudioFilePath)) return;
        if (_currentlyPlayingPart != null)
        {
            _currentlyPlayingPart.IsPlaying = false;
        }
        _currentlyPlayingPart = part;
        part.IsPlaying = true;
        try
        {
            await _audioService.PlayAudioAsync(part.AudioFilePath);
        }
        catch
        {
            part.IsPlaying = false;
            _currentlyPlayingPart = null;
            throw;
        }
    }

    [RelayCommand]
    public async Task StopAudioAsync(SpeakingPart part)
    {
        if (part == null) return;
        _audioService.StopAudio();
        part.IsPlaying = false;
        if (_currentlyPlayingPart == part)
        {
            _currentlyPlayingPart = null;
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task TranscribeAudioAsync(SpeakingPart part)
    {
        if (part == null || string.IsNullOrEmpty(part.AudioFilePath)) return;
        part.IsTranscribing = true;
        IncrementLoading();
        ErrorMessage = null;
        IsErrorVisible = false;
        try
        {
            part.Transcript = await _vertexAIService.TranscribeAudioAsync(part.AudioFilePath);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorVisible = true;
        }
        finally
        {
            part.IsTranscribing = false;
            DecrementLoading();
        }
    }

    private string BuildFormattedAiComment(string justification, IReadOnlyList<string> evidence, IReadOnlyList<string> limitingFactors)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(justification))
        {
            parts.Add($"JUSTIFICATION:\n{justification}");
        }
        if (evidence != null && evidence.Count > 0 && evidence.Any(e => !string.IsNullOrWhiteSpace(e)))
        {
            parts.Add($"EVIDENCE QUOTES:\n• {string.Join("\n• ", evidence.Where(e => !string.IsNullOrWhiteSpace(e)))}");
        }
        if (limitingFactors != null && limitingFactors.Count > 0 && limitingFactors.Any(lf => !string.IsNullOrWhiteSpace(lf)))
        {
            parts.Add($"LIMITING FACTORS:\n• {string.Join("\n• ", limitingFactors.Where(lf => !string.IsNullOrWhiteSpace(lf)))}");
        }
        return string.Join("\n\n", parts);
    }

    private async Task GradeSinglePartInternalAsync(SpeakingPart part)
    {
        if (part == null) throw new ArgumentNullException(nameof(part));
        part.IsGrading = true;
        try
        {
            if (string.IsNullOrWhiteSpace(part.Transcript))
            {
                if (!string.IsNullOrEmpty(part.AudioFilePath))
                {
                    part.IsTranscribing = true;
                    try
                    {
                        part.Transcript = await _vertexAIService.TranscribeAudioAsync(part.AudioFilePath);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Auto-transcription failed for Part {part.PartNumber}: {ex.Message}", ex);
                    }
                    finally
                    {
                        part.IsTranscribing = false;
                    }
                }
                else
                {
                    throw new ArgumentException($"Transcript cannot be empty for Part {part.PartNumber}. Please record speaking, upload an audio file, or enter a transcript manually.");
                }
            }

            int wpm = 0;
            int pauses = 0;
            if (!string.IsNullOrEmpty(part.AudioFilePath))
            {
                var duration = _audioService.GetDuration(part.AudioFilePath);
                pauses = _audioService.EstimateLongPauses(part.AudioFilePath);
                if (duration.TotalMinutes > 0 && !string.IsNullOrEmpty(part.Transcript))
                {
                    int wordCount = part.Transcript.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    wpm = (int)(wordCount / duration.TotalMinutes);
                }
            }

            var result = await _vertexAIService.GradeSpeakingAsync(
                part.Transcript,
                part.PartNumber,
                part.AudioFilePath,
                wpm,
                pauses
            );

            part.FluencyCoherence = result.AnalyticalCriteriaScores.FluencyCoherence.Band;
            part.LexicalResource = result.AnalyticalCriteriaScores.LexicalResource.Band;
            part.GrammaticalRange = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.Band;
            part.Pronunciation = result.AnalyticalCriteriaScores.Pronunciation.Band;

            // Formatted AI comments
            part.FluencyCoherenceAIComment = BuildFormattedAiComment(
                result.AnalyticalCriteriaScores.FluencyCoherence.KeyJustification,
                result.AnalyticalCriteriaScores.FluencyCoherence.SupportingEvidenceQuotes,
                result.AnalyticalCriteriaScores.FluencyCoherence.LimitingFactors
            );
            part.LexicalResourceAIComment = BuildFormattedAiComment(
                result.AnalyticalCriteriaScores.LexicalResource.KeyJustification,
                result.AnalyticalCriteriaScores.LexicalResource.SupportingEvidenceQuotes,
                result.AnalyticalCriteriaScores.LexicalResource.LimitingFactors
            );
            part.GrammaticalRangeAIComment = BuildFormattedAiComment(
                result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification,
                result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.SupportingEvidenceQuotes,
                result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.LimitingFactors
            );
            part.PronunciationAIComment = BuildFormattedAiComment(
                result.AnalyticalCriteriaScores.Pronunciation.KeyJustification,
                result.AnalyticalCriteriaScores.Pronunciation.SupportingEvidenceQuotes,
                result.AnalyticalCriteriaScores.Pronunciation.LimitingFactors
            );

            part.FluencyCoherenceJustification = result.AnalyticalCriteriaScores.FluencyCoherence.KeyJustification;
            part.FluencyCoherenceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.FluencyCoherence.SupportingEvidenceQuotes);
            part.FluencyCoherenceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.FluencyCoherence.LimitingFactors);

            part.LexicalResourceJustification = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            part.LexicalResourceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.SupportingEvidenceQuotes);
            part.LexicalResourceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.LimitingFactors);

            part.GrammaticalRangeJustification = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            part.GrammaticalRangeEvidence = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.SupportingEvidenceQuotes);
            part.GrammaticalRangeLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.LimitingFactors);

            part.PronunciationJustification = result.AnalyticalCriteriaScores.Pronunciation.KeyJustification;
            part.PronunciationEvidence = string.Join("; ", result.AnalyticalCriteriaScores.Pronunciation.SupportingEvidenceQuotes);
            part.PronunciationLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.Pronunciation.LimitingFactors);

            part.CoreStrengths = result.StudentCoaching.CoreStrengths;
            part.PrimaryWeakness = result.StudentCoaching.PrimaryWeaknessToFix;
            part.ActionablePractice = result.StudentCoaching.ActionablePracticeExercise;

            var allMatchedIds = new List<string>();
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.FluencyCoherence.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.LexicalResource.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.MatchedDescriptorIds);
            allMatchedIds.AddRange(result.AnalyticalCriteriaScores.Pronunciation.MatchedDescriptorIds);

            IeltsTeachingAssistant.Models.SelectedRubricDescriptorsExtensions.SetSelectedRubricDescriptors(part, JsonSerializer.Serialize(allMatchedIds));

            // Backup the AI scores
            _speakingAiBackups[part.PartNumber] = (
                part.FluencyCoherence,
                part.LexicalResource,
                part.GrammaticalRange,
                part.Pronunciation
            );

            LoadDescriptorsFromPart(part);
            IsDirty = true;
        }
        finally
        {
            part.IsGrading = false;
        }
    }

    [RelayCommand]
    public async Task GradeWithAiAsync(SpeakingPart part)
    {
        IncrementLoading();
        ErrorMessage = null;
        IsErrorVisible = false;
        try
        {
            await GradeSinglePartInternalAsync(part);
            UpdateAverages();
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

    [RelayCommand]
    public async Task GradeWithAiCentralizedAsync()
    {
        var partsToGrade = new List<SpeakingPart>();
        if (SelectedEvaluationScope == "Whole Test (Parts 1-3)")
        {
            partsToGrade.AddRange(SpeakingParts);
        }
        else if (SelectedEvaluationScope == "Part 1" && SpeakingParts.Count > 0)
        {
            partsToGrade.Add(SpeakingParts[0]);
        }
        else if (SelectedEvaluationScope == "Part 2" && SpeakingParts.Count > 1)
        {
            partsToGrade.Add(SpeakingParts[1]);
        }
        else if (SelectedEvaluationScope == "Part 3" && SpeakingParts.Count > 2)
        {
            partsToGrade.Add(SpeakingParts[2]);
        }

        var gradeTasks = partsToGrade.Where(p => !string.IsNullOrEmpty(p.Transcript) || !string.IsNullOrEmpty(p.AudioFilePath)).ToList();
        if (gradeTasks.Count == 0)
        {
            ErrorMessage = "No transcript or audio recording is available to grade. Please enter a transcript or load audio first.";
            IsErrorVisible = true;
            return;
        }

        IncrementLoading();
        ErrorMessage = null;
        IsErrorVisible = false;

        try
        {
            foreach (var part in gradeTasks)
            {
                await GradeSinglePartInternalAsync(part);
            }
            UpdateAverages();
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

    public void RevertToAiScore(SpeakingPart part)
    {
        if (part == null) return;
        if (_speakingAiBackups.TryGetValue(part.PartNumber, out var scores))
        {
            part.FluencyCoherence = scores.FC;
            part.LexicalResource = scores.LR;
            part.GrammaticalRange = scores.GRA;
            part.Pronunciation = scores.PR;
            
            LoadDescriptorsFromPart(part);
            IsDirty = true;
        }
    }

    public string? CheckForConflicts(SpeakingPart part)
    {
        if (part == null) return null;
        if (_speakingAiBackups.TryGetValue(part.PartNumber, out var scores))
        {
            if (Math.Abs(part.FluencyCoherence - scores.FC) >= 2.0 ||
                Math.Abs(part.LexicalResource - scores.LR) >= 2.0 ||
                Math.Abs(part.GrammaticalRange - scores.GRA) >= 2.0 ||
                Math.Abs(part.Pronunciation - scores.PR) >= 2.0)
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
            var eval = new SpeakingEvaluation
            {
                StudentId = SelectedStudent.Id,
                ClassId = SelectedStudent.ClassId,
                EvaluationMode = SelectedEvaluationMode,
                TestType = TestType.Academic,
                EvaluatedAt = DateTime.UtcNow
            };
            foreach (var part in SpeakingParts)
            {
                eval.Parts.Add(part);
            }
            await _evaluationService.CreateSpeakingEvaluationAsync(eval);
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
        SelectedEvaluationScope = "Whole Test (Parts 1-3)";
        SpeakingParts.Clear();
        SpeakingParts.Add(new SpeakingPart { PartNumber = 1 });
        SpeakingParts.Add(new SpeakingPart { PartNumber = 2 });
        SpeakingParts.Add(new SpeakingPart { PartNumber = 3 });
        SelectedPart = SpeakingParts.FirstOrDefault();
        UpdateVisibleSpeakingParts();

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
        _speakingAiBackups.Clear();
        IsDirty = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    public void GoToNext()
    {
        if (SpeakingParts.Count == 0) return;
        if (SelectedPart == null)
        {
            SelectedPart = SpeakingParts.FirstOrDefault();
        }
        else
        {
            int index = SpeakingParts.IndexOf(SelectedPart);
            if (index >= 0 && index < SpeakingParts.Count - 1)
            {
                SelectedPart = SpeakingParts[index + 1];
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

    private SpeakingEvaluation _evaluation = new();
    public SpeakingEvaluation Evaluation 
    {
        get
        {
            _evaluation.Parts = SpeakingParts;
            return _evaluation;
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public void UpdateAverages()
    {
        OnPropertyChanged(nameof(Evaluation));
    }

    public async Task UploadAudioFileAsync(SpeakingPart part, string path)
    {
        if (part == null) return;
        part.AudioFilePath = path;
        await Task.CompletedTask;
    }
}


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IeltsTeachingAssistant.Data;
using Microsoft.EntityFrameworkCore;

namespace IeltsTeachingAssistant.ViewModels;

public partial class SpeakingEvaluationViewModel : ObservableObject
{
    private readonly IVertexAIService _vertexAiService;
    private readonly IAudioService _audioService;
    private readonly IEvaluationService _evaluationService;
    private readonly AppDbContext _context;

    [ObservableProperty]
    private SpeakingEvaluation _evaluation;

    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private int _activePivotIndex = 0;

    public List<string> EvaluationModes { get; } = new()
    {
        "Full Test (Parts 1-3)",
        "Part 1 Only",
        "Part 2 Only",
        "Part 3 Only"
    };

    [ObservableProperty]
    private string _selectedEvaluationMode = "Full Test (Parts 1-3)";

    partial void OnSelectedEvaluationModeChanged(string value)
    {
        UpdateEvaluationParts();
    }

    private void UpdateEvaluationParts()
    {
        var student = SelectedStudent;
        var newEval = new SpeakingEvaluation
        {
            Student = student,
            StudentId = student?.Id ?? 0,
            EvaluationMode = SelectedEvaluationMode
        };

        if (SelectedEvaluationMode == "Full Test (Parts 1-3)")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = newEval });
            newEval.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = newEval });
            newEval.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 1 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 2 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 3 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = newEval });
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

    public SpeakingEvaluationViewModel(
        IVertexAIService vertexAiService,
        IAudioService audioService,
        IEvaluationService evaluationService,
        AppDbContext context)
    {
        _vertexAiService = vertexAiService;
        _audioService = audioService;
        _evaluationService = evaluationService;
        _context = context;

        _evaluation = new SpeakingEvaluation { EvaluationMode = SelectedEvaluationMode };
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = _evaluation });
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = _evaluation });
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = _evaluation });

        _audioService.PlaybackStopped += (s, e) =>
        {
            if (App.MainWindowInstance != null)
            {
                App.MainWindowInstance.DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var part in _evaluation.Parts)
                    {
                        part.IsPlaying = false;
                    }
                });
            }
        };
    }

    private void ClearSessionInternal(Student? student)
    {
        var newEval = new SpeakingEvaluation
        {
            Student = student,
            StudentId = student?.Id ?? 0,
            EvaluationMode = SelectedEvaluationMode
        };

        if (SelectedEvaluationMode == "Full Test (Parts 1-3)")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = newEval });
            newEval.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = newEval });
            newEval.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 1 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 2 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = newEval });
        }
        else if (SelectedEvaluationMode == "Part 3 Only")
        {
            newEval.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = newEval });
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

    private string GetAudioDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IeltsTeachingAssistant",
            "Audio");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public async Task TranscribePartAudioAsync(SpeakingPart part)
    {
        if (part == null || string.IsNullOrEmpty(part.AudioFilePath)) return;

        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        part.IsTranscribing = true;

        try
        {
            var transcription = await _vertexAiService.TranscribeAudioAsync(part.AudioFilePath);
            part.Transcript = transcription;
            HasUnsavedChanges = true;

            ErrorMessage = $"Transcription completed successfully for Part {part.PartNumber}!";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
            IsErrorVisible = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Transcription failed for Part {part.PartNumber}: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
        finally
        {
            part.IsTranscribing = false;
        }
    }

    public async Task UploadAudioFileAsync(SpeakingPart part, string sourceFilePath)
    {
        if (part == null || string.IsNullOrEmpty(sourceFilePath)) return;

        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        part.IsTranscribing = true;

        try
        {
            var extension = Path.GetExtension(sourceFilePath);
            var destFileName = $"upload_part{part.PartNumber}_{Guid.NewGuid()}{extension}";
            var destFilePath = Path.Combine(GetAudioDirectory(), destFileName);

            File.Copy(sourceFilePath, destFilePath, overwrite: true);
            part.AudioFilePath = destFilePath;
            HasUnsavedChanges = true;

            await TranscribePartAudioAsync(part);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to upload and transcribe audio: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
            part.IsTranscribing = false;
        }
    }

    [RelayCommand]
    private async Task StartRecordingAsync(SpeakingPart part)
    {
        if (part == null) return;

        if (_audioService.IsRecording)
        {
            _audioService.StopRecording();
        }
        _audioService.StopAudio();
        foreach (var p in Evaluation.Parts)
        {
            p.IsPlaying = false;
            p.IsRecording = false;
        }

        try
        {
            var fileName = $"record_part{part.PartNumber}_{Guid.NewGuid()}.wav";
            var fullPath = Path.Combine(GetAudioDirectory(), fileName);

            await _audioService.StartRecordingAsync(fullPath);
            part.IsRecording = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to start recording: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
    }

    [RelayCommand]
    private async Task StopRecordingAsync(SpeakingPart part)
    {
        if (part == null) return;

        try
        {
            var path = _audioService.StopRecording();
            part.IsRecording = false;

            if (!string.IsNullOrEmpty(path))
            {
                part.AudioFilePath = path;
                HasUnsavedChanges = true;
                await TranscribePartAudioAsync(part);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to stop recording: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
    }

    [RelayCommand]
    private async Task PlayAudioAsync(SpeakingPart part)
    {
        if (part == null || string.IsNullOrEmpty(part.AudioFilePath)) return;

        if (_audioService.IsRecording)
        {
            _audioService.StopRecording();
        }
        _audioService.StopAudio();
        foreach (var p in Evaluation.Parts)
        {
            p.IsPlaying = false;
            p.IsRecording = false;
        }

        try
        {
            part.IsPlaying = true;
            await _audioService.PlayAudioAsync(part.AudioFilePath);
        }
        catch (Exception ex)
        {
            part.IsPlaying = false;
            ErrorMessage = $"Failed to play audio: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
    }

    [RelayCommand]
    private void StopAudio(SpeakingPart part)
    {
        if (part == null) return;
        try
        {
            _audioService.StopAudio();
            part.IsPlaying = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to stop audio: {ex.Message}";
            InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            IsErrorVisible = true;
        }
    }

    [RelayCommand]
    private async Task TranscribeAudioAsync(SpeakingPart part)
    {
        await TranscribePartAudioAsync(part);
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
    private async Task GradeWithAiAsync(SpeakingPart part)
    {
        if (part == null) return;

        IsErrorVisible = false;
        ErrorMessage = string.Empty;
        part.IsGrading = true;
        IsGrading = true;
        InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;

        try
        {
            // Calculate telemetry metrics
            int wpm = 0;
            int longPauseCount = 0;
            if (!string.IsNullOrEmpty(part.AudioFilePath) && File.Exists(part.AudioFilePath))
            {
                var duration = _audioService.GetDuration(part.AudioFilePath).TotalSeconds;
                var words = (part.Transcript ?? "").Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
                wpm = duration > 0 ? (int)Math.Round(words / (duration / 60.0)) : 0;
                longPauseCount = _audioService.EstimateLongPauses(part.AudioFilePath);
            }

            var result = await _vertexAiService.GradeSpeakingAsync(part.Transcript ?? "", part.PartNumber, part.AudioFilePath, wpm, longPauseCount);

            part.FluencyCoherence = result.AnalyticalCriteriaScores.FluencyCoherence.Band;
            part.FluencyCoherenceAIComment = result.AnalyticalCriteriaScores.FluencyCoherence.KeyJustification;
            part.FluencyCoherenceJustification = result.AnalyticalCriteriaScores.FluencyCoherence.KeyJustification;
            part.FluencyCoherenceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.FluencyCoherence.SupportingEvidenceQuotes);
            part.FluencyCoherenceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.FluencyCoherence.LimitingFactors);

            part.LexicalResource = result.AnalyticalCriteriaScores.LexicalResource.Band;
            part.LexicalResourceAIComment = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            part.LexicalResourceJustification = result.AnalyticalCriteriaScores.LexicalResource.KeyJustification;
            part.LexicalResourceEvidence = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.SupportingEvidenceQuotes);
            part.LexicalResourceLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.LexicalResource.LimitingFactors);

            part.GrammaticalRange = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.Band;
            part.GrammaticalRangeAIComment = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            part.GrammaticalRangeJustification = result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.KeyJustification;
            part.GrammaticalRangeEvidence = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.SupportingEvidenceQuotes);
            part.GrammaticalRangeLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.GrammaticalRangeAccuracy.LimitingFactors);

            part.Pronunciation = result.AnalyticalCriteriaScores.Pronunciation.Band;
            part.PronunciationAIComment = result.AnalyticalCriteriaScores.Pronunciation.KeyJustification;
            part.PronunciationJustification = result.AnalyticalCriteriaScores.Pronunciation.KeyJustification;
            part.PronunciationEvidence = string.Join("; ", result.AnalyticalCriteriaScores.Pronunciation.SupportingEvidenceQuotes);
            part.PronunciationLimitingFactors = string.Join("; ", result.AnalyticalCriteriaScores.Pronunciation.LimitingFactors);

            part.CoreStrengths = result.StudentCoaching.CoreStrengths;
            part.PrimaryWeakness = result.StudentCoaching.PrimaryWeaknessToFix;
            part.ActionablePractice = result.StudentCoaching.ActionablePracticeExercise;

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
            part.IsGrading = false;
            IsGrading = false;
        }
    }

    public void UpdateAverages()
    {
        OnPropertyChanged(nameof(Evaluation));
    }

    [RelayCommand]
    public async Task GoToNextAsync()
    {
        if (IsFocusMode)
        {
            if (ActivePivotIndex < Evaluation.Parts.Count - 1)
            {
                int nextPartNum = ActivePivotIndex + 2;
                var nextPart = Evaluation.Parts.FirstOrDefault(p => p.PartNumber == nextPartNum);
                if (nextPart != null)
                {
                    nextPart.Transcript = string.Empty;
                    nextPart.AudioFilePath = string.Empty;
                }
                ActivePivotIndex++;
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

            var oldPrompts = Evaluation.Parts.ToDictionary(
                p => p.PartNumber,
                p => (p.Topic, p.CueCard));

            SelectedStudent = nextStudent;

            foreach (var part in Evaluation.Parts)
            {
                if (oldPrompts.TryGetValue(part.PartNumber, out var value))
                {
                    part.Topic = value.Topic;
                    part.CueCard = value.CueCard;
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
                _context.SpeakingEvaluations.Add(Evaluation);
            }
            else
            {
                var entry = _context.Entry(Evaluation);
                if (entry.State == EntityState.Detached)
                {
                    _context.SpeakingEvaluations.Update(Evaluation);
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

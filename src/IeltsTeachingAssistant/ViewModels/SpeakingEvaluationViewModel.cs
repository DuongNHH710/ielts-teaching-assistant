using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;
using System.Collections.ObjectModel;
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

    [ObservableProperty]
    private SpeakingEvaluation _evaluation;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Student> _students = new();

    private readonly AppDbContext _context;

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
        Data.AppDbContext context)
    {
        _vertexAiService = vertexAiService;
        _audioService = audioService;
        _evaluationService = evaluationService;
        _context = context;

        _evaluation = new SpeakingEvaluation();
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 1, Evaluation = _evaluation });
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 2, Evaluation = _evaluation });
        _evaluation.Parts.Add(new SpeakingPart { PartNumber = 3, Evaluation = _evaluation });
        
        // Students are loaded via InitializeAsync()
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

    [RelayCommand]
    private async Task StartRecordingAsync(SpeakingPart part)
    {
        if (part == null) return;
        await _audioService.StartRecordingAsync($"dummy_part{part.PartNumber}.wav");
    }

    [RelayCommand]
    private void StopRecording(SpeakingPart part)
    {
        _audioService.StopRecording();
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
        IsGrading = true;
        InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;

        try
        {
            var result = await _vertexAiService.GradeSpeakingAsync(part.Transcript ?? "", part.PartNumber);
            part.FluencyCoherence = result.FluencyCoherence;
            part.FluencyCoherenceAIComment = result.FluencyCoherenceAIComment;
            part.LexicalResource = result.LexicalResource;
            part.LexicalResourceAIComment = result.LexicalResourceAIComment;
            part.GrammaticalRange = result.GrammaticalRange;
            part.GrammaticalRangeAIComment = result.GrammaticalRangeAIComment;
            part.Pronunciation = result.Pronunciation;
            part.PronunciationAIComment = result.PronunciationAIComment;

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

    public void UpdateAverages()
    {
        OnPropertyChanged(nameof(Evaluation));
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

            // Mark Student and Class as Unchanged so EF Core doesn't try to insert them as new records
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

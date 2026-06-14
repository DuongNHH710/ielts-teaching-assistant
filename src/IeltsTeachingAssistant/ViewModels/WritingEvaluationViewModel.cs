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

public partial class WritingEvaluationViewModel : ObservableObject
{
    private readonly IVertexAIService _vertexAiService;
    private readonly IEvaluationService _evaluationService;

    [ObservableProperty]
    private WritingEvaluation _evaluation;

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

    public WritingEvaluationViewModel(
        IVertexAIService vertexAiService,
        IEvaluationService evaluationService,
        Data.AppDbContext context)
    {
        _vertexAiService = vertexAiService;
        _evaluationService = evaluationService;
        _context = context;

        _evaluation = new WritingEvaluation();
        _evaluation.Tasks.Add(new WritingTask { TaskNumber = 1, Evaluation = _evaluation });
        _evaluation.Tasks.Add(new WritingTask { TaskNumber = 2, Evaluation = _evaluation });
        
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
            task.TaskAchievement = result.TaskAchievement;
            task.TaskAchievementAIComment = result.TaskAchievementAIComment;
            task.CoherenceCohesion = result.CoherenceCohesion;
            task.CoherenceCohesionAIComment = result.CoherenceCohesionAIComment;
            task.LexicalResource = result.LexicalResource;
            task.LexicalResourceAIComment = result.LexicalResourceAIComment;
            task.GrammaticalRange = result.GrammaticalRange;
            task.GrammaticalRangeAIComment = result.GrammaticalRangeAIComment;

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
            // Ensure ClassId is set from the selected Student
            Evaluation.ClassId = Evaluation.Student.ClassId;
            Evaluation.StudentId = Evaluation.Student.Id;
            
            // Unset navigation properties to prevent EF from incorrectly trying to attach/insert them
            var tempStudent = Evaluation.Student;
            var tempClass = Evaluation.Class;
            Evaluation.Student = null;
            Evaluation.Class = null;

            if (Evaluation.Id == 0)
            {
                _context.WritingEvaluations.Add(Evaluation);
            }
            else
            {
                _context.WritingEvaluations.Update(Evaluation);
            }
            await _context.SaveChangesAsync();
            
            // Restore navigation properties
            Evaluation.Student = tempStudent;
            Evaluation.Class = tempClass;
            
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

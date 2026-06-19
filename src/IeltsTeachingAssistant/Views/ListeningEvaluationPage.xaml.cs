using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;
using IeltsTeachingAssistant.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace IeltsTeachingAssistant.Views;

public sealed partial class ListeningEvaluationPage : Page
{
    private readonly IEvaluationService _evaluationService;
    private readonly IVertexAIService _vertexAIService;
    private readonly AppDbContext _context;

    public ListeningEvaluationPage()
    {
        this.InitializeComponent();
        _evaluationService = App.Services.GetRequiredService<IEvaluationService>();
        _vertexAIService = App.Services.GetRequiredService<IVertexAIService>();
        _context = App.Services.GetRequiredService<AppDbContext>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadStudentsAsync();
    }

    private async System.Threading.Tasks.Task LoadStudentsAsync()
    {
        try
        {
            var students = await _context.Students.OrderBy(s => s.Name).ToListAsync();
            StudentCombo.ItemsSource = students;
            StudentCombo.DisplayMemberPath = "Name";
            if (students.Any())
            {
                StudentCombo.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            ShowInfoBar(InfoBarSeverity.Error, $"Failed to load students: {ex.Message}");
        }
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        string script = ScriptInput.Text;
        string answers = AnswersInput.Text;

        if (string.IsNullOrWhiteSpace(script) || string.IsNullOrWhiteSpace(answers))
        {
            ShowInfoBar(InfoBarSeverity.Warning, "Please provide both the listening script context and the student's answers.");
            return;
        }

        AnalyzeButton.IsEnabled = false;
        GradingProgress.IsActive = true;
        ShowInfoBar(InfoBarSeverity.Informational, "Analyzing script and answers with Gemini AI...");

        try
        {
            var analysis = await _vertexAIService.AnalyzeListeningAsync(script, answers);
            AnalysisOutput.Text = analysis;
            ShowInfoBar(InfoBarSeverity.Success, "Analysis completed successfully!");
        }
        catch (Exception ex)
        {
            AnalysisOutput.Text = $"Error during analysis:\n{ex.Message}";
            ShowInfoBar(InfoBarSeverity.Error, $"AI analysis failed: {ex.Message}");
        }
        finally
        {
            AnalyzeButton.IsEnabled = true;
            GradingProgress.IsActive = false;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedStudent = StudentCombo.SelectedItem as Student;
        if (selectedStudent == null)
        {
            ShowInfoBar(InfoBarSeverity.Warning, "Please select a student first.");
            return;
        }

        string script = ScriptInput.Text;
        string answers = AnswersInput.Text;
        string analysis = AnalysisOutput.Text;
        double bandScore = ScoreSlider.Value;

        if (string.IsNullOrWhiteSpace(script) || string.IsNullOrWhiteSpace(answers) || string.IsNullOrWhiteSpace(analysis) || analysis.StartsWith("AI diagnostic"))
        {
            ShowInfoBar(InfoBarSeverity.Warning, "Please run the AI analysis before saving the evaluation.");
            return;
        }

        try
        {
            var eval = new ListeningEvaluation
            {
                StudentId = selectedStudent.Id,
                ClassId = selectedStudent.ClassId,
                ListeningScript = script,
                StudentAnswers = answers,
                BandScore = bandScore,
                DiagnosticAnalysis = analysis,
                EvaluatedAt = DateTime.UtcNow
            };

            await _evaluationService.CreateListeningEvaluationAsync(eval);
            ShowInfoBar(InfoBarSeverity.Success, $"Successfully saved Listening Evaluation for {selectedStudent.Name} with Band {bandScore:F1}!");
        }
        catch (Exception ex)
        {
            ShowInfoBar(InfoBarSeverity.Error, $"Failed to save evaluation: {ex.Message}");
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        AnswersInput.Text = string.Empty;
        AnalysisOutput.Text = "AI diagnostic analysis will appear here after clicking analyze.";
        StatusInfo.IsOpen = false;
    }

    private void ShowInfoBar(InfoBarSeverity severity, string message)
    {
        StatusInfo.Severity = severity;
        StatusInfo.Message = message;
        StatusInfo.IsOpen = true;
    }
}

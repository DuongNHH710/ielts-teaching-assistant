using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services;

namespace IeltsTeachingAssistant.Views
{
    public sealed partial class ReadingEvaluationPage : Page
    {
        private readonly IServiceScope _scope;
        private readonly AppDbContext _dbContext;
        private readonly IVertexAIService _vertexAiService;
        private readonly IEvaluationService _evaluationService;

        public ReadingEvaluationPage()
        {
            _scope = App.Services.CreateScope();
            this.InitializeComponent();
            _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _vertexAiService = _scope.ServiceProvider.GetRequiredService<IVertexAIService>();
            _evaluationService = _scope.ServiceProvider.GetRequiredService<IEvaluationService>();

            LoadStudents();
            this.Unloaded += (s, e) => _scope.Dispose();
        }

        private void LoadStudents()
        {
            try
            {
                var students = _dbContext.Students.ToList();
                StudentCombo.ItemsSource = students;
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading students: " + ex.Message, InfoBarSeverity.Error);
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PassageInput.Text) || string.IsNullOrWhiteSpace(AnswersInput.Text))
            {
                ShowStatus("Please enter both the reading passage and student answers.", InfoBarSeverity.Warning);
                return;
            }

            GradingProgress.IsActive = true;
            AnalyzeButton.IsEnabled = false;
            ShowStatus("Analyzing reading passage and accuracy with AI...", InfoBarSeverity.Informational);

            try
            {
                var analysis = await _vertexAiService.AnalyzeReadingAsync(PassageInput.Text, AnswersInput.Text);
                AnalysisOutput.Text = analysis;
                ShowStatus("Analysis complete.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus("AI Analysis failed: " + ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                GradingProgress.IsActive = false;
                AnalyzeButton.IsEnabled = true;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudent = StudentCombo.SelectedItem as Student;
            if (selectedStudent == null)
            {
                ShowStatus("Please select a student.", InfoBarSeverity.Warning);
                return;
            }

            try
            {
                var eval = new ReadingEvaluation
                {
                    StudentId = selectedStudent.Id,
                    ClassId = selectedStudent.ClassId,
                    ReadingPassage = PassageInput.Text,
                    StudentAnswers = AnswersInput.Text,
                    BandScore = ScoreSlider.Value,
                    DiagnosticAnalysis = AnalysisOutput.Text,
                    EvaluatedAt = DateTime.UtcNow
                };

                await _evaluationService.CreateReadingEvaluationAsync(eval);
                ShowStatus("Evaluation saved successfully.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus("Error saving evaluation: " + ex.Message, InfoBarSeverity.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StudentCombo.SelectedItem = null;
            ScoreSlider.Value = 6.0;
            PassageInput.Text = string.Empty;
            AnswersInput.Text = string.Empty;
            AnalysisOutput.Text = "AI diagnostic analysis will appear here after clicking analyze.";
            StatusInfo.IsOpen = false;
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusInfo.Message = message;
            StatusInfo.Severity = severity;
            StatusInfo.IsOpen = true;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;

namespace IeltsTeachingAssistant.ViewModels;

public partial class StudentPerformanceViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private Student? _student;

    [ObservableProperty]
    private string _studentName = string.Empty;

    [ObservableProperty]
    private string _className = string.Empty;

    [ObservableProperty]
    private double _overallBand;

    [ObservableProperty]
    private double _targetBand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetProgressString))]
    private double _targetProgress;

    public string TargetProgressString => $"{TargetProgress}%";

    [ObservableProperty]
    private double _avgSpeaking;

    [ObservableProperty]
    private double _avgWriting;

    [ObservableProperty]
    private double _avgReading;

    [ObservableProperty]
    private double _avgListening;

    // Averages of individual sub-criteria
    [ObservableProperty]
    private double _speakingFluency;

    [ObservableProperty]
    private double _speakingLexical;

    [ObservableProperty]
    private double _speakingGrammar;

    [ObservableProperty]
    private double _speakingPronunciation;

    [ObservableProperty]
    private double _writingTaskAchievement;

    [ObservableProperty]
    private double _writingCoherence;

    [ObservableProperty]
    private double _writingLexical;

    [ObservableProperty]
    private double _writingGrammar;

    // Charts
    [ObservableProperty]
    private IEnumerable<ISeries> _trendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _xAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _yAxes = Array.Empty<ICartesianAxis>();

    // Evaluation History
    [ObservableProperty]
    private ObservableCollection<EvaluationHistoryItem> _evaluations = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEvaluationTitle))]
    [NotifyPropertyChangedFor(nameof(SelectedEvaluationDateString))]
    [NotifyPropertyChangedFor(nameof(SelectedEvaluationOverallBand))]
    [NotifyPropertyChangedFor(nameof(SelectedEvaluationSubItems))]
    private EvaluationHistoryItem? _selectedEvaluation;

    public string SelectedEvaluationTitle => SelectedEvaluation?.Title ?? string.Empty;
    public string SelectedEvaluationDateString => SelectedEvaluation?.DateString ?? string.Empty;
    public double SelectedEvaluationOverallBand => SelectedEvaluation?.OverallBand ?? 0.0;
    public List<EvaluationSubItem> SelectedEvaluationSubItems =>
        SelectedEvaluation?.SubItems ?? new List<EvaluationSubItem>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EvaluationsListVisibility))]
    [NotifyPropertyChangedFor(nameof(NoEvaluationsLabelVisibility))]
    private bool _hasEvaluations;

    public Microsoft.UI.Xaml.Visibility EvaluationsListVisibility => HasEvaluations ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    public Microsoft.UI.Xaml.Visibility NoEvaluationsLabelVisibility => HasEvaluations ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _studentPhone = string.Empty;

    [ObservableProperty]
    private string _goalPredictionText = string.Empty;

    [ObservableProperty]
    private string _goalPredictionColor = string.Empty;

    public StudentPerformanceViewModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(int studentId)
    {
        IsLoading = true;
        Evaluations.Clear();

        try
        {
            // Load student with all evaluations and sub-parts/tasks
            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.SpeakingEvaluations)
                    .ThenInclude(e => e.Parts)
                .Include(s => s.WritingEvaluations)
                    .ThenInclude(e => e.Tasks)
                .Include(s => s.ReadingEvaluations)
                .Include(s => s.ListeningEvaluations)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            Student = student;
            StudentName = student.Name;
            ClassName = student.Class?.Name ?? "No Class";
            OverallBand = student.OverallBand;
            TargetBand = student.TargetBandScore ?? 0.0;
            TargetProgress = TargetBand > 0 ? Math.Min(100, Math.Round((OverallBand / TargetBand) * 100)) : 0;

            AvgSpeaking = student.AverageSpeakingBand;
            AvgWriting = student.AverageWritingBand;
            AvgReading = student.AverageReadingBand;
            AvgListening = student.AverageListeningBand;

            StudentPhone = student.Phone ?? "No Phone";

            if (TargetBand > 0)
            {
                var diff = TargetBand - OverallBand;
                if (diff <= 0)
                {
                    GoalPredictionText = "Target Met";
                    GoalPredictionColor = "#10B981"; // Emerald
                }
                else if (diff <= 0.5)
                {
                    GoalPredictionText = "High Probability";
                    GoalPredictionColor = "#10B981"; // Emerald
                }
                else if (diff <= 1.0)
                {
                    GoalPredictionText = "Medium Probability";
                    GoalPredictionColor = "#F59E0B"; // Amber
                }
                else
                {
                    GoalPredictionText = "Requires Intervention";
                    GoalPredictionColor = "#F43F5E"; // Rose
                }
            }
            else
            {
                GoalPredictionText = "No Target Set";
                GoalPredictionColor = "#9CA3AF"; // Gray
            }

            // Compute sub-criteria averages for Speaking
            var spEvalsWithParts = student.SpeakingEvaluations.Where(e => e.Parts.Any()).ToList();
            if (spEvalsWithParts.Any())
            {
                SpeakingFluency = Math.Round(spEvalsWithParts.Average(e => e.FluencyCoherence) * 10) / 10.0;
                SpeakingLexical = Math.Round(spEvalsWithParts.Average(e => e.LexicalResource) * 10) / 10.0;
                SpeakingGrammar = Math.Round(spEvalsWithParts.Average(e => e.GrammaticalRange) * 10) / 10.0;
                SpeakingPronunciation = Math.Round(spEvalsWithParts.Average(e => e.Pronunciation) * 10) / 10.0;
            }

            // Compute sub-criteria averages for Writing
            var wrEvalsWithTasks = student.WritingEvaluations.Where(e => e.Tasks.Any()).ToList();
            if (wrEvalsWithTasks.Any())
            {
                WritingTaskAchievement = Math.Round(wrEvalsWithTasks.Average(e => e.TaskAchievement) * 10) / 10.0;
                WritingCoherence = Math.Round(wrEvalsWithTasks.Average(e => e.CoherenceCohesion) * 10) / 10.0;
                WritingLexical = Math.Round(wrEvalsWithTasks.Average(e => e.LexicalResource) * 10) / 10.0;
                WritingGrammar = Math.Round(wrEvalsWithTasks.Average(e => e.GrammaticalRange) * 10) / 10.0;
            }

            // Build Evaluation History List
            var historyItems = new List<EvaluationHistoryItem>();

            foreach (var eval in student.SpeakingEvaluations)
            {
                var historyItem = new EvaluationHistoryItem
                {
                    Id = eval.Id,
                    Type = "Speaking",
                    Icon = "\uE720", // Microphone icon
                    OverallBand = eval.OverallBand,
                    EvaluatedAt = eval.EvaluatedAt
                };

                foreach (var part in eval.Parts.OrderBy(p => p.PartNumber))
                {
                    historyItem.SubItems.Add(new EvaluationSubItem
                    {
                        Title = $"Part {part.PartNumber}",
                        OverallBand = part.OverallBand,
                        Metric1 = part.FluencyCoherence,
                        Metric2 = part.LexicalResource,
                        Metric3 = part.GrammaticalRange,
                        Metric4 = part.Pronunciation,
                        ParentType = "Speaking",
                        Prompt = !string.IsNullOrWhiteSpace(part.Topic) ? $"Topic: {part.Topic}{(string.IsNullOrWhiteSpace(part.CueCard) ? "" : "\nCue Card: " + part.CueCard)}" : "IELTS Speaking Interview Section",
                        SubmissionText = part.Transcript ?? "(No transcript available)",
                        AIComment1 = part.FluencyCoherenceAIComment ?? string.Empty,
                        AIComment2 = part.LexicalResourceAIComment ?? string.Empty,
                        AIComment3 = part.GrammaticalRangeAIComment ?? string.Empty,
                        AIComment4 = part.PronunciationAIComment ?? string.Empty,
                        TeacherComment1 = part.FluencyCoherenceTeacherComment ?? string.Empty,
                        TeacherComment2 = part.LexicalResourceTeacherComment ?? string.Empty,
                        TeacherComment3 = part.GrammaticalRangeTeacherComment ?? string.Empty,
                        TeacherComment4 = part.PronunciationTeacherComment ?? string.Empty
                    });
                }

                historyItems.Add(historyItem);
            }

            foreach (var eval in student.WritingEvaluations)
            {
                var historyItem = new EvaluationHistoryItem
                {
                    Id = eval.Id,
                    Type = "Writing",
                    Icon = "\uE8A5", // Document/Pen icon
                    OverallBand = eval.OverallBand,
                    EvaluatedAt = eval.EvaluatedAt
                };

                foreach (var task in eval.Tasks.OrderBy(t => t.TaskNumber))
                {
                    var taskTypeStr = task.TaskNumber == 1 ? $"Task 1 ({task.TaskType})" : "Task 2 (Essay)";
                    historyItem.SubItems.Add(new EvaluationSubItem
                    {
                        Title = taskTypeStr,
                        OverallBand = task.OverallBand,
                        Metric1 = task.TaskAchievement,
                        Metric2 = task.LexicalResource,
                        Metric3 = task.GrammaticalRange,
                        Metric4 = task.CoherenceCohesion,
                        ParentType = "Writing",
                        Prompt = task.Prompt ?? string.Empty,
                        SubmissionText = task.SubmissionText ?? string.Empty,
                        AIComment1 = task.TaskAchievementAIComment ?? string.Empty,
                        AIComment2 = task.LexicalResourceAIComment ?? string.Empty,
                        AIComment3 = task.GrammaticalRangeAIComment ?? string.Empty,
                        AIComment4 = task.CoherenceCohesionAIComment ?? string.Empty,
                        TeacherComment1 = task.TaskAchievementTeacherComment ?? string.Empty,
                        TeacherComment2 = task.LexicalResourceTeacherComment ?? string.Empty,
                        TeacherComment3 = task.GrammaticalRangeTeacherComment ?? string.Empty,
                        TeacherComment4 = task.CoherenceCohesionTeacherComment ?? string.Empty
                    });
                }

                historyItems.Add(historyItem);
            }

            foreach (var eval in student.ReadingEvaluations)
            {
                var historyItem = new EvaluationHistoryItem
                {
                    Id = eval.Id,
                    Type = "Reading",
                    Icon = "\uE8C9", // Book/Read icon
                    OverallBand = eval.BandScore,
                    EvaluatedAt = eval.EvaluatedAt
                };

                historyItem.SubItems.Add(new EvaluationSubItem
                {
                    Title = "Reading Diagnostic Analysis",
                    OverallBand = eval.BandScore,
                    Metric1 = eval.BandScore,
                    ParentType = "Reading",
                    Prompt = eval.ReadingPassage ?? string.Empty,
                    SubmissionText = eval.StudentAnswers ?? string.Empty,
                    AIComment1 = eval.DiagnosticAnalysis ?? string.Empty
                });

                historyItems.Add(historyItem);
            }

            foreach (var eval in student.ListeningEvaluations)
            {
                var historyItem = new EvaluationHistoryItem
                {
                    Id = eval.Id,
                    Type = "Listening",
                    Icon = "\uE767", // Volume/Listening icon
                    OverallBand = eval.BandScore,
                    EvaluatedAt = eval.EvaluatedAt
                };

                historyItem.SubItems.Add(new EvaluationSubItem
                {
                    Title = "Listening Auditory & Spelling Analysis",
                    OverallBand = eval.BandScore,
                    Metric1 = eval.BandScore,
                    ParentType = "Listening",
                    Prompt = eval.ListeningScript ?? string.Empty,
                    SubmissionText = eval.StudentAnswers ?? string.Empty,
                    AIComment1 = eval.DiagnosticAnalysis ?? string.Empty
                });

                historyItems.Add(historyItem);
            }

            var sortedHistory = historyItems.OrderByDescending(h => h.EvaluatedAt).ToList();
            foreach (var item in sortedHistory)
            {
                Evaluations.Add(item);
            }

            HasEvaluations = Evaluations.Any();
            SelectedEvaluation = Evaluations.FirstOrDefault();

            // Render Chart
            RenderChart(student);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing student performance: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RenderChart(Student student)
    {
        var speakingPoints = student.SpeakingEvaluations
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new { Date = e.EvaluatedAt, Band = e.OverallBand })
            .ToList();

        var writingPoints = student.WritingEvaluations
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new { Date = e.EvaluatedAt, Band = e.OverallBand })
            .ToList();

        var readingPoints = student.ReadingEvaluations
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new { Date = e.EvaluatedAt, Band = e.BandScore })
            .ToList();

        var listeningPoints = student.ListeningEvaluations
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new { Date = e.EvaluatedAt, Band = e.BandScore })
            .ToList();

        var allDates = speakingPoints.Select(p => p.Date)
            .Concat(writingPoints.Select(p => p.Date))
            .Concat(readingPoints.Select(p => p.Date))
            .Concat(listeningPoints.Select(p => p.Date))
            .OrderBy(d => d)
            .Distinct()
            .ToList();

        if (!allDates.Any())
        {
            TrendSeries = Array.Empty<ISeries>();
            return;
        }

        var dateLabels = allDates.Select(d => d.ToString("MM/dd")).ToArray();

        var speakingValues = allDates.Select(d =>
        {
            var match = speakingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault();
            return match?.Band ?? 0.0;
        }).ToArray();

        var writingValues = allDates.Select(d =>
        {
            var match = writingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault();
            return match?.Band ?? 0.0;
        }).ToArray();

        var readingValues = allDates.Select(d =>
        {
            var match = readingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault();
            return match?.Band ?? 0.0;
        }).ToArray();

        var listeningValues = allDates.Select(d =>
        {
            var match = listeningPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault();
            return match?.Band ?? 0.0;
        }).ToArray();

        var overallValues = allDates.Select(d =>
        {
            var spBand = speakingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault()?.Band ?? 0.0;
            var wrBand = writingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault()?.Band ?? 0.0;
            var rdBand = readingPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault()?.Band ?? 0.0;
            var lsBand = listeningPoints.Where(p => p.Date <= d).OrderByDescending(p => p.Date).FirstOrDefault()?.Band ?? 0.0;
            
            var bands = new List<double>();
            if (spBand > 0) bands.Add(spBand);
            if (wrBand > 0) bands.Add(wrBand);
            if (rdBand > 0) bands.Add(rdBand);
            if (lsBand > 0) bands.Add(lsBand);

            return bands.Any() ? bands.Average() : 0.0;
        }).ToArray();

        TrendSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Overall Band",
                Values = overallValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 4), // Blue
                Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(20)),
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            },
            new LineSeries<double>
            {
                Name = "Speaking",
                Values = speakingValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#10B981"), 2), // Emerald
                Fill = null,
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#10B981"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            },
            new LineSeries<double>
            {
                Name = "Writing",
                Values = writingValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 2), // Amber
                Fill = null,
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            },
            new LineSeries<double>
            {
                Name = "Reading",
                Values = readingValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#8B5CF6"), 2), // Purple
                Fill = null,
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#8B5CF6"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            },
            new LineSeries<double>
            {
                Name = "Listening",
                Values = listeningValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#EC4899"), 2), // Pink/Rose
                Fill = null,
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#EC4899"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            }
        };

        XAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = dateLabels,
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(40))
            }
        };

        YAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 9,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(40))
            }
        };
    }
}

public class EvaluationHistoryItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double OverallBand { get; set; }
    public string OverallBandString => OverallBand.ToString("F1");
    public DateTime EvaluatedAt { get; set; }
    public string DateString => EvaluatedAt.ToString("MMM d, yyyy HH:mm");
    public string Title => Type switch
    {
        "Speaking" => "Speaking Evaluation",
        "Writing" => "Writing Evaluation",
        "Reading" => "Reading Diagnostic",
        "Listening" => "Listening Diagnostic",
        _ => "Evaluation"
    };
    public List<EvaluationSubItem> SubItems { get; set; } = new();
}

public class EvaluationSubItem
{
    public string Title { get; set; } = string.Empty;
    public double OverallBand { get; set; }
    public double Metric1 { get; set; }
    public double Metric2 { get; set; }
    public double Metric3 { get; set; }
    public double Metric4 { get; set; }

    public string ParentType { get; set; } = string.Empty;

    public string Label1 => Title.Contains("Part") ? "Fluency & Coherence" : "Task Achievement";
    public string Label2 => "Lexical Resource";
    public string Label3 => "Grammatical Range & Accuracy";
    public string Label4 => Title.Contains("Part") ? "Pronunciation" : "Coherence & Cohesion";

    public string Prompt { get; set; } = string.Empty;
    public string SubmissionText { get; set; } = string.Empty;

    public string AIComment1 { get; set; } = string.Empty;
    public string AIComment2 { get; set; } = string.Empty;
    public string AIComment3 { get; set; } = string.Empty;
    public string AIComment4 { get; set; } = string.Empty;

    public string TeacherComment1 { get; set; } = string.Empty;
    public string TeacherComment2 { get; set; } = string.Empty;
    public string TeacherComment3 { get; set; } = string.Empty;
    public string TeacherComment4 { get; set; } = string.Empty;

    public Microsoft.UI.Xaml.Visibility MetricsVisibility => (ParentType == "Speaking" || ParentType == "Writing") ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    public Microsoft.UI.Xaml.Visibility DiagnosticVisibility => (ParentType == "Reading" || ParentType == "Listening") ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    public Microsoft.UI.Xaml.Visibility PromptVisibility => string.IsNullOrWhiteSpace(Prompt) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    public Microsoft.UI.Xaml.Visibility AIComment1Visibility => string.IsNullOrWhiteSpace(AIComment1) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility AIComment2Visibility => string.IsNullOrWhiteSpace(AIComment2) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility AIComment3Visibility => string.IsNullOrWhiteSpace(AIComment3) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility AIComment4Visibility => string.IsNullOrWhiteSpace(AIComment4) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    public Microsoft.UI.Xaml.Visibility TeacherComment1Visibility => string.IsNullOrWhiteSpace(TeacherComment1) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility TeacherComment2Visibility => string.IsNullOrWhiteSpace(TeacherComment2) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility TeacherComment3Visibility => string.IsNullOrWhiteSpace(TeacherComment3) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
    public Microsoft.UI.Xaml.Visibility TeacherComment4Visibility => string.IsNullOrWhiteSpace(TeacherComment4) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
}

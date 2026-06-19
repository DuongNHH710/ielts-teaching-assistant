using CommunityToolkit.Mvvm.ComponentModel;
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

public partial class ClassPerformanceViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClassProgress))]
    private ClassEntity? _classEntity;

    public float ClassProgress => ClassEntity?.Progress ?? 0f;

    [ObservableProperty]
    private string _className = string.Empty;

    [ObservableProperty]
    private int _studentCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClassOverallBandString))]
    private double _classOverallBand;

    public string ClassOverallBandString => ClassOverallBand.ToString("F1");

    [ObservableProperty]
    private int _totalEvaluations;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetAchievementRateString))]
    private double _targetAchievementRate; // % of students meeting/exceeding target

    public string TargetAchievementRateString => $"{TargetAchievementRate}%";

    [ObservableProperty]
    private double _classAvgSpeaking;

    [ObservableProperty]
    private double _classAvgWriting;

    [ObservableProperty]
    private double _classAvgReading;

    [ObservableProperty]
    private double _classAvgListening;

    // Student Roster
    [ObservableProperty]
    private ObservableCollection<ClassStudentItem> _students = new();

    // Recent Evaluations in Class
    [ObservableProperty]
    private ObservableCollection<ClassRecentEvalItem> _recentEvaluations = new();

    // Charts
    [ObservableProperty]
    private IEnumerable<ISeries> _distributionSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _distributionXAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _distributionYAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private IEnumerable<ISeries> _classTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _classTrendXAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _classTrendYAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _syllabusWeekText = string.Empty;

    public ClassPerformanceViewModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(int classId)
    {
        IsLoading = true;
        Students.Clear();
        RecentEvaluations.Clear();

        try
        {
            // Load class with students and their evaluations
            var classEntity = await _context.Classes
                .Include(c => c.Students)
                    .ThenInclude(s => s.SpeakingEvaluations)
                .Include(c => c.Students)
                    .ThenInclude(s => s.WritingEvaluations)
                .Include(c => c.Students)
                    .ThenInclude(s => s.ReadingEvaluations)
                .Include(c => c.Students)
                    .ThenInclude(s => s.ListeningEvaluations)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null) return;

            ClassEntity = classEntity;
            ClassName = classEntity.Name;
            StudentCount = classEntity.Students.Count;
            ClassOverallBand = classEntity.OverallBand;

            ClassAvgSpeaking = classEntity.AverageSpeakingBand;
            ClassAvgWriting = classEntity.AverageWritingBand;
            ClassAvgReading = classEntity.AverageReadingBand;
            ClassAvgListening = classEntity.AverageListeningBand;

            // Compute Syllabus Week text
            int sessionsPerWeek = classEntity.SessionsPerWeek > 0 ? classEntity.SessionsPerWeek : 2;
            int totalWeeks = (int)Math.Ceiling((double)classEntity.TotalSessions / sessionsPerWeek);
            int currentWeek = (classEntity.SessionsCompleted / sessionsPerWeek) + 1;
            if (currentWeek > totalWeeks) currentWeek = totalWeeks;
            if (currentWeek < 1) currentWeek = 1;

            var topic = currentWeek switch
            {
                1 => "Intro & Fundamentals",
                2 => "Speaking Part 1 & Vocabulary",
                3 => "Writing Task 1 Data Analysis",
                4 => "Reading Scanning & Skimming",
                5 => "Listening Phonemes & Dictation",
                6 => "Writing Task 2 Essay Structuring",
                7 => "Speaking Part 2 Narrative Flow",
                8 => "Grammatical Range & Accuracy",
                9 => "Speaking Part 3 Abstract Discussion",
                10 => "Writing Cohesion & Coherence",
                11 => "Full Mock Test & Review",
                _ => "Final Strategy & Graduation"
            };
            SyllabusWeekText = $"Week {currentWeek} of {totalWeeks}: {topic}";

            // Load all evaluations for this class specifically
            var speakingEvals = await _context.SpeakingEvaluations
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId)
                .ToListAsync();

            var writingEvals = await _context.WritingEvaluations
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId)
                .ToListAsync();

            var readingEvals = await _context.ReadingEvaluations
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId)
                .ToListAsync();

            var listeningEvals = await _context.ListeningEvaluations
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId)
                .ToListAsync();

            TotalEvaluations = speakingEvals.Count + writingEvals.Count + readingEvals.Count + listeningEvals.Count;

            // Calculate Target Achievement Rate
            var activeStudents = classEntity.Students.ToList();
            if (activeStudents.Any())
            {
                var studentsMeetingTarget = activeStudents
                    .Count(s => s.TargetBandScore.HasValue && s.OverallBand >= s.TargetBandScore.Value);
                TargetAchievementRate = Math.Round((double)studentsMeetingTarget / activeStudents.Count * 100);
            }

            // Populate Student Roster
            foreach (var student in activeStudents.OrderByDescending(s => s.OverallBand))
            {
                Students.Add(new ClassStudentItem
                {
                    Id = student.Id,
                    Name = student.Name,
                    CurrentBand = student.OverallBand,
                    TargetBand = student.TargetBandScore ?? 0.0,
                    SpeakingBand = student.AverageSpeakingBand,
                    WritingBand = student.AverageWritingBand,
                    ReadingBand = student.AverageReadingBand,
                    ListeningBand = student.AverageListeningBand
                });
            }

            // Populate Recent Evaluations
            var recentSpeaking = speakingEvals
                .Select(e => new ClassRecentEvalItem
                {
                    StudentName = e.Student?.Name ?? "Unknown Student",
                    Type = "Speaking",
                    Icon = "\uE720",
                    OverallBand = e.OverallBand,
                    EvaluatedAt = e.EvaluatedAt
                });

            var recentWriting = writingEvals
                .Select(e => new ClassRecentEvalItem
                {
                    StudentName = e.Student?.Name ?? "Unknown Student",
                    Type = "Writing",
                    Icon = "\uE8A5",
                    OverallBand = e.OverallBand,
                    EvaluatedAt = e.EvaluatedAt
                });

            var recentReading = readingEvals
                .Select(e => new ClassRecentEvalItem
                {
                    StudentName = e.Student?.Name ?? "Unknown Student",
                    Type = "Reading",
                    Icon = "\uE8C9",
                    OverallBand = e.BandScore,
                    EvaluatedAt = e.EvaluatedAt
                });

            var recentListening = listeningEvals
                .Select(e => new ClassRecentEvalItem
                {
                    StudentName = e.Student?.Name ?? "Unknown Student",
                    Type = "Listening",
                    Icon = "\uE767",
                    OverallBand = e.BandScore,
                    EvaluatedAt = e.EvaluatedAt
                });

            var combinedSorted = recentSpeaking
                .Concat(recentWriting)
                .Concat(recentReading)
                .Concat(recentListening)
                .OrderByDescending(e => e.EvaluatedAt)
                .Take(15);

            foreach (var item in combinedSorted)
            {
                RecentEvaluations.Add(item);
            }

            // Build Charts
            RenderDistributionChart(activeStudents);
            RenderTrendChart(speakingEvals, writingEvals, readingEvals, listeningEvals);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing class performance: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RenderDistributionChart(List<Student> students)
    {
        var bands = new double[] { 4.0, 4.5, 5.0, 5.5, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0 };
        var distributionCounts = new double[bands.Length];

        foreach (var student in students)
        {
            var roundedBand = Math.Round(student.OverallBand * 2) / 2.0;
            var idx = Array.IndexOf(bands, roundedBand);
            if (idx >= 0)
            {
                distributionCounts[idx]++;
            }
        }

        DistributionSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Students",
                Values = distributionCounts,
                Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")), // Blue
                Stroke = null,
                MaxBarWidth = 32
            }
        };

        DistributionXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = bands.Select(b => b.ToString("F1")).ToArray(),
                SeparatorsPaint = null
            }
        };

        DistributionYAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(30))
            }
        };
    }

    private void RenderTrendChart(
        List<SpeakingEvaluation> speakingEvals,
        List<WritingEvaluation> writingEvals,
        List<ReadingEvaluation> readingEvals,
        List<ListeningEvaluation> listeningEvals)
    {
        var allEvals = speakingEvals.Select(e => new { e.EvaluatedAt, OverallBand = e.OverallBand })
            .Concat(writingEvals.Select(e => new { e.EvaluatedAt, OverallBand = e.OverallBand }))
            .Concat(readingEvals.Select(e => new { e.EvaluatedAt, OverallBand = e.BandScore }))
            .Concat(listeningEvals.Select(e => new { e.EvaluatedAt, OverallBand = e.BandScore }))
            .OrderBy(e => e.EvaluatedAt)
            .ToList();

        if (!allEvals.Any())
        {
            ClassTrendSeries = Array.Empty<ISeries>();
            return;
        }

        // Group by day to show average class progress
        var dailyPoints = allEvals
            .GroupBy(e => e.EvaluatedAt.Date)
            .Select(g => new { Date = g.Key, Avg = g.Average(e => e.OverallBand) })
            .OrderBy(p => p.Date)
            .ToList();

        var dateLabels = dailyPoints.Select(p => p.Date.ToString("MM/dd")).ToArray();
        var avgValues = dailyPoints.Select(p => Math.Round(p.Avg * 2) / 2.0).ToArray();

        // Project trend line
        var projectedValues = new double[avgValues.Length];
        for (int i = 0; i < avgValues.Length; i++)
        {
            projectedValues[i] = Math.Min(9.0, avgValues[i] + (i * 0.12));
        }

        ClassTrendSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Actual Avg Band",
                Values = avgValues,
                Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")), // Blue Column
                MaxBarWidth = 24
            },
            new LineSeries<double>
            {
                Name = "AI Projected Trend",
                Values = projectedValues,
                Stroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 3) { PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 6, 4 }) }, // Amber dashed line
                Fill = null,
                GeometrySize = 6,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            }
        };

        ClassTrendXAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = dateLabels,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(30))
            }
        };

        ClassTrendYAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 9,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(30))
            }
        };
    }
}

public class ClassStudentItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double CurrentBand { get; set; }
    public double TargetBand { get; set; }
    public double SpeakingBand { get; set; }
    public double WritingBand { get; set; }
    public double ReadingBand { get; set; }
    public double ListeningBand { get; set; }

    public double GoalProbability
    {
        get
        {
            if (TargetBand <= 0) return 100;
            if (CurrentBand >= TargetBand) return 98;
            var diff = TargetBand - CurrentBand;
            if (diff <= 0.5) return 85;
            if (diff <= 1.0) return 60;
            if (diff <= 1.5) return 35;
            return 15;
        }
    }

    public string GoalProbabilityText
    {
        get
        {
            var prob = GoalProbability;
            if (prob >= 80) return "High";
            if (prob >= 50) return "Medium";
            return "Low";
        }
    }

    public string GoalProbabilityPercentageString => $"({(int)GoalProbability}%)";

    public string GoalProbabilityColor
    {
        get
        {
            var prob = GoalProbability;
            if (prob >= 80) return "#10B981"; // Emerald
            if (prob >= 50) return "#F59E0B"; // Amber
            return "#F43F5E"; // Rose
        }
    }

    public string StatusText => TargetBand > 0 ? (CurrentBand >= TargetBand ? "Target Reached" : "On Track") : "No Target Set";
    public string StatusColor => TargetBand > 0 ? (CurrentBand >= TargetBand ? "#10B981" : "#3B82F6") : "#9CA3AF";

    public Microsoft.UI.Xaml.Media.Brush GoalProbabilityBrush
    {
        get
        {
            var hex = GoalProbabilityColor.TrimStart('#');
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, r, g, b));
            }
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
        }
    }
}

public class ClassRecentEvalItem
{
    public string StudentName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double OverallBand { get; set; }
    public string OverallBandString => OverallBand.ToString("F1");
    public DateTime EvaluatedAt { get; set; }
    public string DateString => EvaluatedAt.ToString("MMM d, HH:mm");
}

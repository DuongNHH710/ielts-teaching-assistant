using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using System.Collections.ObjectModel;

namespace IeltsTeachingAssistant.ViewModels;

/// <summary>
/// ViewModel for the Dashboard page. Provides overview statistics,
/// recent evaluations, and quick action commands.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private int _totalStudents;

    [ObservableProperty]
    private int _activeClasses;

    [ObservableProperty]
    private int _monthlyEvaluations;

    [ObservableProperty]
    private float _averageBand;

    [ObservableProperty]
    private ObservableCollection<RecentEvaluationItem> _recentEvaluations = new();

    public DashboardViewModel(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads all dashboard statistics and recent activity from the database.
    /// </summary>
    public async Task LoadDashboardDataAsync()
    {
        try
        {
            TotalStudents = await _context.Students.CountAsync();

            ActiveClasses = await _context.Classes
                .Where(c => c.Status == Models.ClassStatus.Active)
                .CountAsync();

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var speakingCount = await _context.SpeakingEvaluations
                .Where(e => e.EvaluatedAt >= startOfMonth)
                .CountAsync();

            var writingCount = await _context.WritingEvaluations
                .Where(e => e.EvaluatedAt >= startOfMonth)
                .CountAsync();

            MonthlyEvaluations = speakingCount + writingCount;

            // Calculate average band across all evaluations
            var speakingAvg = await _context.SpeakingEvaluations
                .Select(e => (float?)e.OverallBand)
                .AverageAsync() ?? 0;

            var writingAvg = await _context.WritingEvaluations
                .Select(e => (float?)e.OverallBand)
                .AverageAsync() ?? 0;

            if (speakingAvg > 0 && writingAvg > 0)
                AverageBand = (speakingAvg + writingAvg) / 2f;
            else if (speakingAvg > 0)
                AverageBand = speakingAvg;
            else
                AverageBand = writingAvg;

            // Load recent evaluations
            await LoadRecentEvaluationsAsync();
        }
        catch (Exception)
        {
            // Graceful degradation — show zeros
        }
    }

    /// <summary>
    /// Loads the 10 most recent evaluations (both Speaking and Writing).
    /// </summary>
    private async Task LoadRecentEvaluationsAsync()
    {
        RecentEvaluations.Clear();

        var speakingEvals = await _context.SpeakingEvaluations
            .Include(e => e.Student)
            .OrderByDescending(e => e.EvaluatedAt)
            .Take(10)
            .Select(e => new RecentEvaluationItem
            {
                StudentName = e.Student!.Name,
                EvaluationType = "Speaking",
                BandScore = e.OverallBand.ToString("F1"),
                Date = e.EvaluatedAt.ToString("MMM d"),
                EvaluatedAt = e.EvaluatedAt
            })
            .ToListAsync();

        var writingEvals = await _context.WritingEvaluations
            .Include(e => e.Student)
            .OrderByDescending(e => e.EvaluatedAt)
            .Take(10)
            .Select(e => new RecentEvaluationItem
            {
                StudentName = e.Student!.Name,
                EvaluationType = "Writing",
                BandScore = e.OverallBand.ToString("F1"),
                Date = e.EvaluatedAt.ToString("MMM d"),
                EvaluatedAt = e.EvaluatedAt
            })
            .ToListAsync();

        var combined = speakingEvals
            .Concat(writingEvals)
            .OrderByDescending(e => e.EvaluatedAt)
            .Take(10);

        foreach (var item in combined)
        {
            RecentEvaluations.Add(item);
        }
    }
}

/// <summary>
/// Display model for recent evaluation items on the dashboard.
/// </summary>
public class RecentEvaluationItem
{
    public string StudentName { get; set; } = string.Empty;
    public string EvaluationType { get; set; } = string.Empty;
    public string BandScore { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
}

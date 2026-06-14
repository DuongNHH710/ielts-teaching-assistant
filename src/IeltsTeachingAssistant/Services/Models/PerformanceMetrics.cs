namespace IeltsTeachingAssistant.Services.Models;

public class PerformanceMetrics
{
    public double AverageSpeakingBand { get; set; }
    public double AverageWritingBand { get; set; }
    
    // Average scores for radar chart
    public double SpeakingFluency { get; set; }
    public double SpeakingLexical { get; set; }
    public double SpeakingGrammar { get; set; }
    public double SpeakingPronunciation { get; set; }

    public double WritingTaskAchievement { get; set; }
    public double WritingCoherence { get; set; }
    public double WritingLexical { get; set; }
    public double WritingGrammar { get; set; }
    
    // Time-series data for line chart
    public List<double> SpeakingTrend { get; set; } = new();
    public List<double> WritingTrend { get; set; } = new();
    public List<string> Labels { get; set; } = new();
}

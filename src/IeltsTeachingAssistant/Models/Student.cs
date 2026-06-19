using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a student enrolled in a class.
/// </summary>
public class Student
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public int ClassId { get; set; }
    public virtual ClassEntity? Class { get; set; }

    public double? TargetBandScore { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public double AverageReadingBand => ReadingEvaluations.Any() ? Math.Round(ReadingEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;
    public double AverageListeningBand => ListeningEvaluations.Any() ? Math.Round(ListeningEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;

    public double AverageSpeakingBand => SpeakingEvaluations.Any() ? Math.Round(SpeakingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;
    public double AverageWritingBand => WritingEvaluations.Any() ? Math.Round(WritingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;

    public double OverallBand
    {
        get
        {
            var bands = new List<double>();
            if (AverageSpeakingBand > 0) bands.Add(AverageSpeakingBand);
            if (AverageWritingBand > 0) bands.Add(AverageWritingBand);
            if (AverageReadingBand > 0) bands.Add(AverageReadingBand);
            if (AverageListeningBand > 0) bands.Add(AverageListeningBand);

            if (bands.Count == 0) return 0;
            return Math.Round(bands.Average() * 2) / 2.0;
        }
    }

    [NotMapped]
    public string Trend => "+0.5";

    public virtual ICollection<SpeakingEvaluation> SpeakingEvaluations { get; set; } = new List<SpeakingEvaluation>();
    public virtual ICollection<WritingEvaluation> WritingEvaluations { get; set; } = new List<WritingEvaluation>();
    public virtual ICollection<ReadingEvaluation> ReadingEvaluations { get; set; } = new List<ReadingEvaluation>();
    public virtual ICollection<ListeningEvaluation> ListeningEvaluations { get; set; } = new List<ListeningEvaluation>();
}

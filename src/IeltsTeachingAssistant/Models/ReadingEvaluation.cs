using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a reading evaluation session for a student.
/// </summary>
public class ReadingEvaluation
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public int ClassId { get; set; }
    public virtual ClassEntity? Class { get; set; }

    public string? ReadingPassage { get; set; }
    public string? StudentAnswers { get; set; }

    public double BandScore { get; set; }

    public string? DiagnosticAnalysis { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

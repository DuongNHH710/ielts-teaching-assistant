using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a listening evaluation session for a student.
/// </summary>
public class ListeningEvaluation
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public int ClassId { get; set; }
    public virtual ClassEntity? Class { get; set; }

    public string? ListeningScript { get; set; }
    public string? StudentAnswers { get; set; }

    public double BandScore { get; set; }

    public string? DiagnosticAnalysis { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

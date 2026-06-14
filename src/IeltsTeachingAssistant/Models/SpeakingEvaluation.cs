using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a speaking evaluation session for a student.
/// </summary>
public class SpeakingEvaluation
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public int ClassId { get; set; }
    public virtual ClassEntity? Class { get; set; }

    public TestType TestType { get; set; }

    public double OverallBand => Parts.Any() ? Math.Round(Parts.Average(p => (p.FluencyCoherence + p.LexicalResource + p.GrammaticalRange + p.Pronunciation) / 4.0) * 2) / 2.0 : 0;
    
    public double FluencyCoherence => Parts.Any() ? Parts.Average(p => p.FluencyCoherence) : 0;
    public double LexicalResource => Parts.Any() ? Parts.Average(p => p.LexicalResource) : 0;
    public double GrammaticalRange => Parts.Any() ? Parts.Average(p => p.GrammaticalRange) : 0;
    public double Pronunciation => Parts.Any() ? Parts.Average(p => p.Pronunciation) : 0;

    public string? Transcript { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<SpeakingPart> Parts { get; set; } = new List<SpeakingPart>();
}

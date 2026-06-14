using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a customizable feedback template.
/// </summary>
public class FeedbackTemplate
{
    [Key]
    public int Id { get; set; }

    public string Category { get; set; } = string.Empty;
    public string Criterion { get; set; } = string.Empty;
    public string BandRange { get; set; } = string.Empty;
    public string TemplateText { get; set; } = string.Empty;
}

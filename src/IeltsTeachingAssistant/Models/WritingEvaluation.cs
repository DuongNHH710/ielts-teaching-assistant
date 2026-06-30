using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a writing evaluation session for a student.
/// </summary>
public class WritingEvaluation
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public int ClassId { get; set; }
    public virtual ClassEntity? Class { get; set; }

    public TestType TestType { get; set; }
    public string? EvaluationMode { get; set; }

    public double OverallBand
    {
        get
        {
            if (!Tasks.Any()) return 0;
            var task1 = Tasks.FirstOrDefault(t => t.TaskNumber == 1)?.OverallBand ?? 0;
            var task2 = Tasks.FirstOrDefault(t => t.TaskNumber == 2)?.OverallBand ?? 0;
            return Helpers.BandScoreCalculator.CalculateWritingOverall(task1, task2);
        }
    }

    public double TaskAchievement => Tasks.Any(t => t.TaskAchievement > 0) ? Tasks.Where(t => t.TaskAchievement > 0).Average(t => t.TaskAchievement) : 0;
    public double CoherenceCohesion => Tasks.Any(t => t.CoherenceCohesion > 0) ? Tasks.Where(t => t.CoherenceCohesion > 0).Average(t => t.CoherenceCohesion) : 0;
    public double LexicalResource => Tasks.Any(t => t.LexicalResource > 0) ? Tasks.Where(t => t.LexicalResource > 0).Average(t => t.LexicalResource) : 0;
    public double GrammaticalRange => Tasks.Any(t => t.GrammaticalRange > 0) ? Tasks.Where(t => t.GrammaticalRange > 0).Average(t => t.GrammaticalRange) : 0;

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<WritingTask> Tasks { get; set; } = new List<WritingTask>();
}

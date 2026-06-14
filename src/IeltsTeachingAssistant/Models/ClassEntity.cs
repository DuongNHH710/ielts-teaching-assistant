using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a class or teaching group.
/// </summary>
public class ClassEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int TotalSessions { get; set; }
    
    public int SessionsPerWeek { get; set; }
    
    public int SessionsCompleted { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    public string? Duration { get; set; }
    
    public string? WeeklySchedule { get; set; }

    public ClassStatus Status { get; set; }
    
    public ClassMode Mode { get; set; }

    public string? OnlineLink { get; set; }
    public string? OnlinePassword { get; set; }
    public string? OnlineHostKey { get; set; }

    public TestType TestType { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    // Computed properties
    [NotMapped]
    public float Progress => TotalSessions == 0 ? 0 : (float)SessionsCompleted / TotalSessions * 100;

    [NotMapped]
    public DateTime EstimatedEndDate
    {
        get
        {
            if (SessionsPerWeek == 0 || TotalSessions == 0) return StartDate;
            var weeks = (int)Math.Ceiling((double)TotalSessions / SessionsPerWeek);
            return StartDate.AddDays(weeks * 7);
        }
    }

    [NotMapped]
    public double AverageReadingBand => Students.Any() ? Math.Round(Students.Where(s => s.AverageReadingBand > 0).Select(s => s.AverageReadingBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;
    
    [NotMapped]
    public double AverageListeningBand => Students.Any() ? Math.Round(Students.Where(s => s.AverageListeningBand > 0).Select(s => s.AverageListeningBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;

    [NotMapped]
    public double AverageSpeakingBand => Students.Any() ? Math.Round(Students.Where(s => s.AverageSpeakingBand > 0).Select(s => s.AverageSpeakingBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;
    
    [NotMapped]
    public double AverageWritingBand => Students.Any() ? Math.Round(Students.Where(s => s.AverageWritingBand > 0).Select(s => s.AverageWritingBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;
    
    [NotMapped]
    public double OverallBand => Students.Any() ? Math.Round(Students.Where(s => s.OverallBand > 0).Select(s => s.OverallBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;

    [NotMapped]
    public string Trend => "+0.5";
}

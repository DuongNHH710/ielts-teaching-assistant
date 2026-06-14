using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Data;

/// <summary>
/// Entity Framework Core database context for the application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ClassEntity> Classes { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<SpeakingEvaluation> SpeakingEvaluations { get; set; } = null!;
    public DbSet<SpeakingPart> SpeakingParts { get; set; } = null!;
    public DbSet<WritingEvaluation> WritingEvaluations { get; set; } = null!;
    public DbSet<WritingTask> WritingTasks { get; set; } = null!;
    public DbSet<FeedbackTemplate> FeedbackTemplates { get; set; } = null!;
    public DbSet<AppSettings> Settings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Class -> Student (One-to-Many)
        modelBuilder.Entity<ClassEntity>()
            .HasMany(c => c.Students)
            .WithOne(s => s.Class)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student -> SpeakingEvaluation (One-to-Many)
        modelBuilder.Entity<Student>()
            .HasMany(s => s.SpeakingEvaluations)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Class -> SpeakingEvaluation (One-to-Many)
        modelBuilder.Entity<SpeakingEvaluation>()
            .HasOne(e => e.Class)
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.NoAction);

        // SpeakingEvaluation -> SpeakingPart (One-to-Many)
        modelBuilder.Entity<SpeakingEvaluation>()
            .HasMany(e => e.Parts)
            .WithOne(p => p.Evaluation)
            .HasForeignKey(p => p.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student -> WritingEvaluation (One-to-Many)
        modelBuilder.Entity<Student>()
            .HasMany(s => s.WritingEvaluations)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Class -> WritingEvaluation (One-to-Many)
        modelBuilder.Entity<WritingEvaluation>()
            .HasOne(e => e.Class)
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.NoAction);

        // WritingEvaluation -> WritingTask (One-to-Many)
        modelBuilder.Entity<WritingEvaluation>()
            .HasMany(e => e.Tasks)
            .WithOne(t => t.Evaluation)
            .HasForeignKey(t => t.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

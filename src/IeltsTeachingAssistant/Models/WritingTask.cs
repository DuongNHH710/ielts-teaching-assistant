using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a specific task (Task 1 or Task 2) in a writing evaluation.
/// </summary>
public partial class WritingTask : ObservableObject
{
    [Key]
    public int Id { get; set; }

    public int EvaluationId { get; set; }
    public virtual WritingEvaluation? Evaluation { get; set; }

    [ObservableProperty]
    private int _taskNumber;

    [ObservableProperty]
    private string _taskType = string.Empty;

    [ObservableProperty]
    private string? _prompt;

    [ObservableProperty]
    private string? _submissionText;

    [ObservableProperty]
    private string? _originalFilePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _taskAchievement;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _coherenceCohesion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _lexicalResource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _grammaticalRange;

    public double OverallBand => Helpers.BandScoreCalculator.CalculateWritingTaskOverall(TaskAchievement, CoherenceCohesion, LexicalResource, GrammaticalRange);

    [ObservableProperty]
    private string? _taskAchievementAIComment;

    [ObservableProperty]
    private string? _taskAchievementTeacherComment;

    [ObservableProperty]
    private string? _coherenceCohesionAIComment;

    [ObservableProperty]
    private string? _coherenceCohesionTeacherComment;

    [ObservableProperty]
    private string? _lexicalResourceAIComment;

    [ObservableProperty]
    private string? _lexicalResourceTeacherComment;

    [ObservableProperty]
    private string? _grammaticalRangeAIComment;

    [ObservableProperty]
    private string? _grammaticalRangeTeacherComment;

    [ObservableProperty]
    private string? _taskAchievementJustification;
    [ObservableProperty]
    private string? _taskAchievementEvidence;
    [ObservableProperty]
    private string? _taskAchievementLimitingFactors;

    [ObservableProperty]
    private string? _coherenceCohesionJustification;
    [ObservableProperty]
    private string? _coherenceCohesionEvidence;
    [ObservableProperty]
    private string? _coherenceCohesionLimitingFactors;

    [ObservableProperty]
    private string? _lexicalResourceJustification;
    [ObservableProperty]
    private string? _lexicalResourceEvidence;
    [ObservableProperty]
    private string? _lexicalResourceLimitingFactors;

    [ObservableProperty]
    private string? _grammaticalRangeJustification;
    [ObservableProperty]
    private string? _grammaticalRangeEvidence;
    [ObservableProperty]
    private string? _grammaticalRangeLimitingFactors;

    [ObservableProperty]
    private string? _coreStrengths;
    [ObservableProperty]
    private string? _primaryWeakness;
    [ObservableProperty]
    private string? _actionablePractice;

    // ── Rubric grid highlight data (not persisted) ────────────────────────────
    // These are populated by the ViewModel immediately after AI grading and
    // drive the RubricGridPanel highlight state.

    private IReadOnlyList<string>? _rubricHighlightIds0;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<string>? RubricHighlightIds0
    {
        get => _rubricHighlightIds0;
        set => SetProperty(ref _rubricHighlightIds0, value);
    }

    private IReadOnlyList<string>? _rubricHighlightIds1;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<string>? RubricHighlightIds1
    {
        get => _rubricHighlightIds1;
        set => SetProperty(ref _rubricHighlightIds1, value);
    }

    private IReadOnlyList<string>? _rubricHighlightIds2;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<string>? RubricHighlightIds2
    {
        get => _rubricHighlightIds2;
        set => SetProperty(ref _rubricHighlightIds2, value);
    }

    private IReadOnlyList<string>? _rubricHighlightIds3;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<string>? RubricHighlightIds3
    {
        get => _rubricHighlightIds3;
        set => SetProperty(ref _rubricHighlightIds3, value);
    }
}

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
    private double _taskAchievement;

    [ObservableProperty]
    private double _coherenceCohesion;

    [ObservableProperty]
    private double _lexicalResource;

    [ObservableProperty]
    private double _grammaticalRange;

    public double OverallBand => Math.Round((TaskAchievement + CoherenceCohesion + LexicalResource + GrammaticalRange) / 4.0 * 2) / 2.0;

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
}

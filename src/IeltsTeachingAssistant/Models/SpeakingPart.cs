using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents a specific part (1, 2, or 3) of a speaking evaluation.
/// </summary>
public partial class SpeakingPart : ObservableObject
{
    [Key]
    public int Id { get; set; }

    public int EvaluationId { get; set; }
    public virtual SpeakingEvaluation? Evaluation { get; set; }

    [ObservableProperty]
    private int _partNumber;

    [ObservableProperty]
    private string? _audioFilePath;

    [ObservableProperty]
    private string? _topic;
    
    [ObservableProperty]
    private string? _cueCard;
    
    [ObservableProperty]
    private string? _transcript;

    [ObservableProperty]
    private double _fluencyCoherence;

    [ObservableProperty]
    private double _lexicalResource;

    [ObservableProperty]
    private double _grammaticalRange;

    [ObservableProperty]
    private double _pronunciation;

    [ObservableProperty]
    private string? _fluencyCoherenceAIComment;

    [ObservableProperty]
    private string? _fluencyCoherenceTeacherComment;

    [ObservableProperty]
    private string? _lexicalResourceAIComment;

    [ObservableProperty]
    private string? _lexicalResourceTeacherComment;

    [ObservableProperty]
    private string? _grammaticalRangeAIComment;

    [ObservableProperty]
    private string? _grammaticalRangeTeacherComment;

    [ObservableProperty]
    private string? _pronunciationAIComment;

    [ObservableProperty]
    private string? _pronunciationTeacherComment;
}

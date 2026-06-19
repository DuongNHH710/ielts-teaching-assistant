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

    public double OverallBand => Math.Round((FluencyCoherence + LexicalResource + GrammaticalRange + Pronunciation) / 4.0 * 2) / 2.0;

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

    [ObservableProperty]
    private string? _fluencyCoherenceJustification;
    [ObservableProperty]
    private string? _fluencyCoherenceEvidence;
    [ObservableProperty]
    private string? _fluencyCoherenceLimitingFactors;

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
    private string? _pronunciationJustification;
    [ObservableProperty]
    private string? _pronunciationEvidence;
    [ObservableProperty]
    private string? _pronunciationLimitingFactors;

    [ObservableProperty]
    private string? _coreStrengths;
    [ObservableProperty]
    private string? _primaryWeakness;
    [ObservableProperty]
    private string? _actionablePractice;

    private bool _isTranscribing;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsTranscribing
    {
        get => _isTranscribing;
        set => SetProperty(ref _isTranscribing, value);
    }

    private bool _isRecording;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
    }

    private bool _isPlaying;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    private bool _isGrading;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsGrading
    {
        get => _isGrading;
        set => SetProperty(ref _isGrading, value);
    }
}

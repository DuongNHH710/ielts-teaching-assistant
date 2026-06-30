using System.Collections.Generic;
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
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _fluencyCoherence;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _lexicalResource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _grammaticalRange;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallBand))]
    private double _pronunciation;

    public double OverallBand => Helpers.BandScoreCalculator.CalculateSpeakingOverall(FluencyCoherence, LexicalResource, GrammaticalRange, Pronunciation);

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

    // ── Rubric grid highlight data (not persisted) ────────────────────────────
    // Populated by the ViewModel after AI grading to drive RubricGridPanel.

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

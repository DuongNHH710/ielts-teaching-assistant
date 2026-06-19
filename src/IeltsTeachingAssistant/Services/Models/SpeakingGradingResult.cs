using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IeltsTeachingAssistant.Services.Models;

public class SpeakingGradingResult
{
    [JsonPropertyName("overall_calculated_band")]
    public float OverallCalculatedBand { get; set; }

    [JsonPropertyName("analytical_criteria_scores")]
    public AnalyticalCriteriaScoresSpeaking AnalyticalCriteriaScores { get; set; } = new();

    [JsonPropertyName("student_coaching")]
    public StudentCoaching StudentCoaching { get; set; } = new();
}

public class AnalyticalCriteriaScoresSpeaking
{
    [JsonPropertyName("fluency_coherence")]
    public CriterionDetail FluencyCoherence { get; set; } = new();

    [JsonPropertyName("lexical_resource")]
    public CriterionDetail LexicalResource { get; set; } = new();

    [JsonPropertyName("grammatical_range_accuracy")]
    public CriterionDetail GrammaticalRangeAccuracy { get; set; } = new();

    [JsonPropertyName("pronunciation")]
    public CriterionDetail Pronunciation { get; set; } = new();
}

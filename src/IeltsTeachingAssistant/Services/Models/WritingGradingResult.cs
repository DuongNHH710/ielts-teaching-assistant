using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IeltsTeachingAssistant.Services.Models;

public class WritingGradingResult
{
    [JsonPropertyName("essay_prompt_provided")]
    public string EssayPromptProvided { get; set; } = string.Empty;

    [JsonPropertyName("calculated_overall_writing_band")]
    public float CalculatedOverallWritingBand { get; set; }

    [JsonPropertyName("analytical_criteria_scores")]
    public AnalyticalCriteriaScoresWriting AnalyticalCriteriaScores { get; set; } = new();

    [JsonPropertyName("student_coaching")]
    public StudentCoaching StudentCoaching { get; set; } = new();
}

public class AnalyticalCriteriaScoresWriting
{
    [JsonPropertyName("task_response")]
    public CriterionDetail TaskResponse { get; set; } = new();

    [JsonPropertyName("coherence_cohesion")]
    public CriterionDetail CoherenceCohesion { get; set; } = new();

    [JsonPropertyName("lexical_resource")]
    public CriterionDetail LexicalResource { get; set; } = new();

    [JsonPropertyName("grammatical_range_accuracy")]
    public CriterionDetail GrammaticalRangeAccuracy { get; set; } = new();
}

public class CriterionDetail
{
    [JsonPropertyName("band")]
    public float Band { get; set; }

    [JsonPropertyName("key_justification")]
    public string KeyJustification { get; set; } = string.Empty;

    [JsonPropertyName("supporting_evidence_quotes")]
    public List<string> SupportingEvidenceQuotes { get; set; } = new();

    [JsonPropertyName("limiting_factors")]
    public List<string> LimitingFactors { get; set; } = new();
}

public class StudentCoaching
{
    [JsonPropertyName("core_strengths")]
    public string CoreStrengths { get; set; } = string.Empty;

    [JsonPropertyName("primary_weakness_to_fix")]
    public string PrimaryWeaknessToFix { get; set; } = string.Empty;

    [JsonPropertyName("actionable_practice_exercise")]
    public string ActionablePracticeExercise { get; set; } = string.Empty;
}

namespace IeltsTeachingAssistant.Services.Models;

public class SpeakingGradingResult
{
    public float FluencyCoherence { get; set; }
    public string FluencyCoherenceAIComment { get; set; } = string.Empty;

    public float LexicalResource { get; set; }
    public string LexicalResourceAIComment { get; set; } = string.Empty;

    public float GrammaticalRange { get; set; }
    public string GrammaticalRangeAIComment { get; set; } = string.Empty;

    public float Pronunciation { get; set; }
    public string PronunciationAIComment { get; set; } = string.Empty;
}

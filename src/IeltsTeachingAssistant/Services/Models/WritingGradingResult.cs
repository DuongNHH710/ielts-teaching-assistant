namespace IeltsTeachingAssistant.Services.Models;

public class WritingGradingResult
{
    public float TaskAchievement { get; set; } // Or Task Response for Task 2
    public string TaskAchievementAIComment { get; set; } = string.Empty;

    public float CoherenceCohesion { get; set; }
    public string CoherenceCohesionAIComment { get; set; } = string.Empty;

    public float LexicalResource { get; set; }
    public string LexicalResourceAIComment { get; set; } = string.Empty;

    public float GrammaticalRange { get; set; }
    public string GrammaticalRangeAIComment { get; set; } = string.Empty;
}

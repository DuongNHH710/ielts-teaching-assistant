using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Helpers;

/// <summary>
/// Helper class for calculating IELTS band scores based on official rules.
/// </summary>
public static class BandScoreCalculator
{
    /// <summary>
    /// Rounds a given score to the nearest 0.5 band.
    /// Official rule: 
    /// .25 -> .5
    /// .75 -> next whole band
    /// </summary>
    public static double RoundToHalfBand(double score)
    {
        double floor = Math.Floor(score);
        double fraction = score - floor;

        if (fraction >= 0.75)
            return floor + 1.0;
        if (fraction >= 0.25)
            return floor + 0.5;
            
        return floor;
    }

    /// <summary>
    /// Calculates overall speaking band score from four criteria.
    /// </summary>
    public static double CalculateSpeakingOverall(double fluency, double lexical, double grammar, double pronunciation)
    {
        double average = (fluency + lexical + grammar + pronunciation) / 4.0;
        return RoundToHalfBand(average);
    }

    /// <summary>
    /// Calculates overall writing task band score from four criteria.
    /// </summary>
    public static double CalculateWritingTaskOverall(double taskAchievement, double coherence, double lexical, double grammar)
    {
        double average = (taskAchievement + coherence + lexical + grammar) / 4.0;
        return RoundToHalfBand(average);
    }

    /// <summary>
    /// Calculates total writing score from Task 1 and Task 2.
    /// Task 2 carries roughly twice the weight of Task 1.
    /// </summary>
    public static double CalculateWritingOverall(double task1Band, double task2Band)
    {
        double weightedAverage = (task1Band + (task2Band * 2)) / 3.0;
        return RoundToHalfBand(weightedAverage);
    }

    /// <summary>
    /// Averages across all speaking parts, then calculates the overall band.
    /// </summary>
    public static double CalculateSpeakingOverallFromParts(List<SpeakingPart> parts)
    {
        if (parts == null || !parts.Any()) return 0;

        double fluencyAvg = parts.Average(p => p.FluencyCoherence);
        double lexicalAvg = parts.Average(p => p.LexicalResource);
        double grammarAvg = parts.Average(p => p.GrammaticalRange);
        double pronunciationAvg = parts.Average(p => p.Pronunciation);

        return CalculateSpeakingOverall(fluencyAvg, lexicalAvg, grammarAvg, pronunciationAvg);
    }
}

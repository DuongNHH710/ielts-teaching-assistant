namespace IeltsTeachingAssistant.Helpers;

/// <summary>
/// Static class containing public IELTS band descriptors.
/// Only partial descriptors provided for brevity.
/// </summary>
public static class IeltsBandDescriptors
{
    // A simplified dictionary to hold some sample descriptors.
    // In a full app, this would be heavily populated.
    // Criterion -> (Band -> Descriptor)
    public static readonly Dictionary<string, Dictionary<float, string>> SpeakingDescriptors = new()
    {
        { "Fluency and Coherence", new Dictionary<float, string>
            {
                { 9.0f, "speaks fluently with only rare repetition or self-correction; any hesitation is content-related rather than to find words or grammar..." },
                { 8.0f, "speaks fluently with only occasional repetition or self-correction; hesitation is usually content-related and only rarely to search for language..." },
                { 7.0f, "speaks at length without noticeable effort or loss of coherence; may demonstrate language-related hesitation at times, or some repetition and/or self-correction..." },
                { 6.0f, "is willing to speak at length, though may lose coherence at times due to occasional repetition, self-correction or hesitation..." }
            }
        },
        // More criteria would go here
    };

    public static readonly Dictionary<string, Dictionary<float, string>> WritingTask2Descriptors = new()
    {
        { "Task Response", new Dictionary<float, string>
            {
                { 9.0f, "fully addresses all parts of the task; presents a fully developed position in answer to the question with relevant, fully extended and well supported ideas." },
                { 8.0f, "sufficiently addresses all parts of the task; presents a well-developed response to the question with relevant, extended and supported ideas." },
                { 7.0f, "addresses all parts of the task; presents a clear position throughout the response; presents, extends and supports main ideas, but there may be a tendency to over-generalise and/or supporting ideas may lack focus." },
                { 6.0f, "addresses all parts of the task although some parts may be more fully covered than others; presents a relevant position although the conclusions may become unclear or repetitive..." }
            }
        }
        // More criteria would go here
    };
}

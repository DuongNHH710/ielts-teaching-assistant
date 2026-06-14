using IeltsTeachingAssistant.Services.Models;

namespace IeltsTeachingAssistant.Services;

public interface IVertexAIService
{
    Task<string> TranscribeAudioAsync(string audioFilePath);
    Task<SpeakingGradingResult> GradeSpeakingAsync(string transcript, int partNumber);
    Task<WritingGradingResult> GradeWritingAsync(string promptText, string text, int taskNumber, string testType, string taskType);
    Task<string> ExtractTextFromImageAsync(string imagePath);
}

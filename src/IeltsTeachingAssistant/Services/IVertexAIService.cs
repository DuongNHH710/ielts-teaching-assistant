using IeltsTeachingAssistant.Services.Models;

namespace IeltsTeachingAssistant.Services;

public interface IVertexAIService
{
    Task<string> TranscribeAudioAsync(string audioFilePath);
    Task<SpeakingGradingResult> GradeSpeakingAsync(string transcript, int partNumber, string? audioFilePath, int wpmRate = 0, int longPauseCount = 0);
    Task<WritingGradingResult> GradeWritingAsync(string promptText, string text, int taskNumber, string testType, string taskType);
    Task<string> AnalyzeReadingAsync(string passage, string studentAnswers);
    Task<string> AnalyzeListeningAsync(string script, string studentAnswers);
    Task<string> ExtractTextFromPdfOrImageAsync(string filePath);
}

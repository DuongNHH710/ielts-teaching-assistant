using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services.Models;

namespace IeltsTeachingAssistant.Services;

public interface IEvaluationService
{
    Task CreateSpeakingEvaluationAsync(SpeakingEvaluation eval);
    Task CreateWritingEvaluationAsync(WritingEvaluation eval);
    Task<List<SpeakingEvaluation>> GetSpeakingEvaluationsForStudentAsync(int studentId);
    Task<List<WritingEvaluation>> GetWritingEvaluationsForStudentAsync(int studentId);
    Task<List<dynamic>> GetRecentEvaluationsAsync(int count);
    Task<PerformanceMetrics> GetStudentPerformanceAsync(int studentId);
    Task DeleteEvaluationAsync(int id, string type);
}

using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Services;

public interface IExportService
{
    Task ExportStudentReportPdfAsync(int studentId, string outputPath);
    Task ExportEvaluationResultsExcelAsync(List<int> evaluationIds, string outputPath);
    Task<List<Student>> ImportStudentsFromExcelAsync(string filePath);
    Task<List<ClassEntity>> ImportClassesFromExcelAsync(string filePath);
    Task<List<Student>> ImportStudentsFromCsvAsync(string filePath);
}

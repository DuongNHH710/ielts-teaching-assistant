using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Services;

public class ExportService : IExportService
{
    // Dummy implementation. QuestPDF and ClosedXML implementation goes here.
    public async Task ExportStudentReportPdfAsync(int studentId, string outputPath)
    {
        await Task.CompletedTask;
    }

    public async Task ExportEvaluationResultsExcelAsync(List<int> evaluationIds, string outputPath)
    {
        await Task.CompletedTask;
    }

    public async Task<List<Student>> ImportStudentsFromExcelAsync(string filePath)
    {
        return await Task.FromResult(new List<Student>());
    }

    public async Task<List<ClassEntity>> ImportClassesFromExcelAsync(string filePath)
    {
        return await Task.FromResult(new List<ClassEntity>());
    }

    public async Task<List<Student>> ImportStudentsFromCsvAsync(string filePath)
    {
        return await Task.FromResult(new List<Student>());
    }
}

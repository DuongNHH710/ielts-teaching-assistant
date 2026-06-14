using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services.Models;

namespace IeltsTeachingAssistant.Services;

public class EvaluationService : IEvaluationService
{
    private readonly AppDbContext _context;

    public EvaluationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateSpeakingEvaluationAsync(SpeakingEvaluation eval)
    {
        _context.SpeakingEvaluations.Add(eval);
        await _context.SaveChangesAsync();
    }

    public async Task CreateWritingEvaluationAsync(WritingEvaluation eval)
    {
        _context.WritingEvaluations.Add(eval);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SpeakingEvaluation>> GetSpeakingEvaluationsForStudentAsync(int studentId)
    {
        return await _context.SpeakingEvaluations
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Parts)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();
    }

    public async Task<List<WritingEvaluation>> GetWritingEvaluationsForStudentAsync(int studentId)
    {
        return await _context.WritingEvaluations
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Tasks)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();
    }

    public async Task<List<dynamic>> GetRecentEvaluationsAsync(int count)
    {
        // Combined in DashboardViewModel already, but this is an alternative
        return await Task.FromResult(new List<dynamic>());
    }

    public async Task<PerformanceMetrics> GetStudentPerformanceAsync(int studentId)
    {
        var metrics = new PerformanceMetrics();
        
        var hasSpeaking = await _context.SpeakingEvaluations.AnyAsync(e => e.StudentId == studentId);
        if (hasSpeaking)
        {
            var speakingQuery = _context.SpeakingEvaluations.AsNoTracking().Where(e => e.StudentId == studentId);
            metrics.AverageSpeakingBand = await speakingQuery.AverageAsync(s => s.OverallBand);
            metrics.SpeakingFluency = await speakingQuery.AverageAsync(s => s.FluencyCoherence);
            metrics.SpeakingLexical = await speakingQuery.AverageAsync(s => s.LexicalResource);
            metrics.SpeakingGrammar = await speakingQuery.AverageAsync(s => s.GrammaticalRange);
            metrics.SpeakingPronunciation = await speakingQuery.AverageAsync(s => s.Pronunciation);
            metrics.SpeakingTrend = await speakingQuery.OrderBy(s => s.EvaluatedAt).Select(s => s.OverallBand).ToListAsync();
        }

        var hasWriting = await _context.WritingEvaluations.AnyAsync(e => e.StudentId == studentId);
        if (hasWriting)
        {
            var writingQuery = _context.WritingEvaluations.AsNoTracking().Where(e => e.StudentId == studentId);
            metrics.AverageWritingBand = await writingQuery.AverageAsync(w => w.OverallBand);
            metrics.WritingTrend = await writingQuery.OrderBy(w => w.EvaluatedAt).Select(w => w.OverallBand).ToListAsync();
        }

        return metrics;
    }

    public async Task DeleteEvaluationAsync(int id, string type)
    {
        if (type == "Speaking")
        {
            var eval = await _context.SpeakingEvaluations.FindAsync(id);
            if (eval != null)
            {
                _context.SpeakingEvaluations.Remove(eval);
                await _context.SaveChangesAsync();
            }
        }
        else if (type == "Writing")
        {
            var eval = await _context.WritingEvaluations.FindAsync(id);
            if (eval != null)
            {
                _context.WritingEvaluations.Remove(eval);
                await _context.SaveChangesAsync();
            }
        }
    }
}

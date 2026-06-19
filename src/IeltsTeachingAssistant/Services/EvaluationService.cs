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

    public async Task CreateReadingEvaluationAsync(ReadingEvaluation eval)
    {
        _context.ReadingEvaluations.Add(eval);
        await _context.SaveChangesAsync();
    }

    public async Task CreateListeningEvaluationAsync(ListeningEvaluation eval)
    {
        _context.ListeningEvaluations.Add(eval);
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

    public async Task<List<ReadingEvaluation>> GetReadingEvaluationsForStudentAsync(int studentId)
    {
        return await _context.ReadingEvaluations
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();
    }

    public async Task<List<ListeningEvaluation>> GetListeningEvaluationsForStudentAsync(int studentId)
    {
        return await _context.ListeningEvaluations
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();
    }

    public async Task<List<dynamic>> GetRecentEvaluationsAsync(int count)
    {
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

        var hasReading = await _context.ReadingEvaluations.AnyAsync(e => e.StudentId == studentId);
        if (hasReading)
        {
            var readingQuery = _context.ReadingEvaluations.AsNoTracking().Where(e => e.StudentId == studentId);
            metrics.AverageReadingBand = await readingQuery.AverageAsync(r => r.BandScore);
            metrics.ReadingTrend = await readingQuery.OrderBy(r => r.EvaluatedAt).Select(r => r.BandScore).ToListAsync();
        }

        var hasListening = await _context.ListeningEvaluations.AnyAsync(e => e.StudentId == studentId);
        if (hasListening)
        {
            var listeningQuery = _context.ListeningEvaluations.AsNoTracking().Where(e => e.StudentId == studentId);
            metrics.AverageListeningBand = await listeningQuery.AverageAsync(l => l.BandScore);
            metrics.ListeningTrend = await listeningQuery.OrderBy(l => l.EvaluatedAt).Select(l => l.BandScore).ToListAsync();
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
        else if (type == "Reading")
        {
            var eval = await _context.ReadingEvaluations.FindAsync(id);
            if (eval != null)
            {
                _context.ReadingEvaluations.Remove(eval);
                await _context.SaveChangesAsync();
            }
        }
        else if (type == "Listening")
        {
            var eval = await _context.ListeningEvaluations.FindAsync(id);
            if (eval != null)
            {
                _context.ListeningEvaluations.Remove(eval);
                await _context.SaveChangesAsync();
            }
        }
    }
}

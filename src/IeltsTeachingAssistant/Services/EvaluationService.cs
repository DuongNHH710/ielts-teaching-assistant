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
            var speakingList = await speakingQuery.Include(e => e.Parts).ToListAsync();
            metrics.AverageSpeakingBand = speakingList.Any() ? speakingList.Average(s => s.OverallBand) : 0;
            metrics.SpeakingFluency = speakingList.Any() ? speakingList.Average(s => s.FluencyCoherence) : 0;
            metrics.SpeakingLexical = speakingList.Any() ? speakingList.Average(s => s.LexicalResource) : 0;
            metrics.SpeakingGrammar = speakingList.Any() ? speakingList.Average(s => s.GrammaticalRange) : 0;
            metrics.SpeakingPronunciation = speakingList.Any() ? speakingList.Average(s => s.Pronunciation) : 0;
            metrics.SpeakingTrend = speakingList.OrderBy(s => s.EvaluatedAt).Select(s => s.OverallBand).ToList();
        }

        var hasWriting = await _context.WritingEvaluations.AnyAsync(e => e.StudentId == studentId);
        if (hasWriting)
        {
            var writingQuery = _context.WritingEvaluations.AsNoTracking().Where(e => e.StudentId == studentId);
            var writingList = await writingQuery.Include(e => e.Tasks).ToListAsync();
            metrics.AverageWritingBand = writingList.Any() ? writingList.Average(w => w.OverallBand) : 0;
            metrics.WritingTrend = writingList.OrderBy(w => w.EvaluatedAt).Select(w => w.OverallBand).ToList();
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

using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using IeltsTeachingAssistant.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IeltsTeachingAssistant.Services;

public class VertexAIService : IVertexAIService
{
    private readonly ILogger<VertexAIService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    
    private static Google.Apis.Auth.OAuth2.GoogleCredential? _cachedCredential;
    private static string? _cachedCredentialsPath;

    public VertexAIService(ILogger<VertexAIService> logger, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<AppSettings?> GetSettingsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Settings.FirstOrDefaultAsync();
    }

    private async Task<string?> GetAccessTokenAsync(string credentialsPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(credentialsPath) || !File.Exists(credentialsPath))
                return null;

            if (_cachedCredential == null || _cachedCredentialsPath != credentialsPath)
            {
                _cachedCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialsPath)
                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                _cachedCredentialsPath = credentialsPath;
            }
            
            var token = await _cachedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Google Cloud access token.");
            return null;
        }
    }

    private async Task<string?> CallGeminiAsync(string systemPrompt, string userPrompt)
    {
        var settings = await GetSettingsAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.GcpProjectId) || string.IsNullOrWhiteSpace(settings.GcpCredentialsPath))
        {
            _logger.LogWarning("Vertex AI settings are not fully configured.");
            return null; // Fallback to mock
        }

        var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
        if (token == null) return null;

        string projectId = settings.GcpProjectId;
        string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
        string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash-preview-0409" : settings.PreferredModel;
        if (modelId == "Gemini 2.5 Flash") modelId = "gemini-2.5-flash";
        if (modelId == "Gemini 2.5 Pro") modelId = "gemini-2.5-pro";

        string endpoint = $"https://{region}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{region}/publishers/google/models/{modelId}:generateContent";

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userPrompt } } }
            },
            generationConfig = new { responseMimeType = "application/json" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string err = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Gemini API error: {response.StatusCode} - {err}");
            return null;
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonResponse);
        
        try
        {
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response.");
            return null;
        }
    }

    public async Task<string> TranscribeAudioAsync(string audioFilePath)
    {
        _logger.LogInformation($"Transcribing audio: {audioFilePath}");
        // Note: For real audio transcription, we would base64 encode the audio file and send it to Gemini 1.5 Flash.
        // For simplicity in this demo, we simulate it if we don't implement full audio upload.
        await Task.Delay(1500); 
        return "This is a simulated transcription. To implement full audio, base64 encode the file and add it to the 'parts' array with mime_type 'audio/wav'.";
    }

    public async Task<SpeakingGradingResult> GradeSpeakingAsync(string transcript, int partNumber)
    {
        _logger.LogInformation("Grading speaking...");
        
        string systemPrompt = "You are an expert IELTS examiner. Grade the provided student transcript for Speaking Part " + partNumber + " based on the official IELTS Speaking Band Descriptors (Public Version). " +
            "Evaluate across the 4 criteria: \n" +
            "1. Fluency and Coherence (speaking at length, flow, cohesion, hesitation).\n" +
            "2. Lexical Resource (range, accuracy, flexibility, idiomatic language).\n" +
            "3. Grammatical Range and Accuracy (complex structures, error density).\n" +
            "4. Pronunciation (clarity, intonation, rhythm, stress).\n" +
            "CRITICAL RULES: \n" +
            "- Be highly accurate and strict, reflecting a real IELTS examiner.\n" +
            "- For each criterion's comment, provide DETAILED, ACTIONABLE feedback (at least 2-3 sentences) explaining EXACTLY why they got that score, pointing out specific errors or strengths from the transcript, and giving concrete advice on how to improve to the next band.\n" +
            "Return ONLY a JSON object with these keys: FluencyCoherence (number 0-9), FluencyCoherenceAIComment (string), LexicalResource (number 0-9), LexicalResourceAIComment (string), GrammaticalRange (number 0-9), GrammaticalRangeAIComment (string), Pronunciation (number 0-9), PronunciationAIComment (string). Use 0.5 increments.";
        string userPrompt = $"Transcript:\n{transcript}";

        string? resultJson = await CallGeminiAsync(systemPrompt, userPrompt);

        if (resultJson != null)
        {
            try
            {
                resultJson = resultJson.Replace("```json", "").Replace("```", "").Trim();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<SpeakingGradingResult>(resultJson, options);
                if (result != null) return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON from Gemini.");
                throw new InvalidOperationException("Failed to parse the AI grading response. Please try again.", ex);
            }
        }

        throw new InvalidOperationException("The AI grading service did not return a valid response. Please check your internet connection or Vertex AI credentials.");
    }

    public async Task<WritingGradingResult> GradeWritingAsync(string promptText, string text, int taskNumber, string testType, string taskType)
    {
        _logger.LogInformation("Grading writing...");
        
        string systemPrompt = $"You are an expert, strict IELTS examiner. Grade the provided {testType} student writing for {taskType} (Task {taskNumber}) based on the official IELTS Writing Band Descriptors (Public Version). " +
            "Evaluate across the 4 criteria: \n" +
            "1. Task Achievement / Task Response (addressing the prompt, providing a clear overview or position, developing ideas).\n" +
            "2. Coherence and Cohesion (logical organization, paragraphing, cohesive devices).\n" +
            "3. Lexical Resource (vocabulary range, accuracy, word choice, spelling).\n" +
            "4. Grammatical Range and Accuracy (sentence structures, punctuation, error density).\n" +
            "CRITICAL RULES: \n" +
            "- Be extremely strict and accurate according to official IELTS public band descriptors.\n" +
            "- If the writing is very short (e.g. one sentence) or fails to address the prompt fully, score it heavily down (Band 1-3) across all criteria. Do not give average scores (5-6) for incomplete or single-sentence answers.\n" +
            "- For each criterion's comment, provide DETAILED, ACTIONABLE feedback (at least 2-3 sentences) explaining EXACTLY why they got that score. Point out specific errors, quote strengths or weaknesses from the text, and give concrete advice on how to improve to the next band.\n" +
            "Return ONLY a JSON object with these keys: TaskAchievement (number 0-9, use TaskResponse for Task 2 but map it to TaskAchievement key), TaskAchievementAIComment (string), CoherenceCohesion (number 0-9), CoherenceCohesionAIComment (string), LexicalResource (number 0-9), LexicalResourceAIComment (string), GrammaticalRange (number 0-9), GrammaticalRangeAIComment (string). Use 0.5 increments.";
        string userPrompt = $"Prompt:\n{promptText}\n\nStudent Writing:\n{text}";

        string? resultJson = await CallGeminiAsync(systemPrompt, userPrompt);

        if (resultJson != null)
        {
            try
            {
                resultJson = resultJson.Replace("```json", "").Replace("```", "").Trim();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<WritingGradingResult>(resultJson, options);
                if (result != null) return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON from Gemini.");
                throw new InvalidOperationException("Failed to parse the AI grading response. Please try again.", ex);
            }
        }

        throw new InvalidOperationException("The AI grading service did not return a valid response. Please check your internet connection or Vertex AI credentials.");
    }

    public async Task<string> ExtractTextFromImageAsync(string imagePath)
    {
        _logger.LogInformation($"Extracting text from image: {imagePath}");
        await Task.Delay(1500); 
        return "Simulated extracted text from image upload. (Gemini Vision integration goes here)";
    }
}

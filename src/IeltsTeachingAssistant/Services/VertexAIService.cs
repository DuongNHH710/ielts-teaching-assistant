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

    private async Task<string?> GetAccessTokenAsync(string? credentialsPath)
    {
        try
        {
            if (_cachedCredential == null || _cachedCredentialsPath != credentialsPath)
            {
                if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
                {
                    _cachedCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialsPath)
                        .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                }
                else
                {
                    _cachedCredential = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
                    if (_cachedCredential.IsCreateScopedRequired)
                    {
                        _cachedCredential = _cachedCredential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                    }
                }
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
        if (settings == null || string.IsNullOrWhiteSpace(settings.GcpProjectId))
        {
            _logger.LogWarning("Vertex AI settings are not fully configured. Project ID is missing.");
            return null;
        }

        var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
        if (token == null) return null;

        string projectId = settings.GcpProjectId;
        string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
        string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash-preview-0409" : settings.PreferredModel;

        // Map old or UI-friendly names to actual Vertex AI endpoints
        if (modelId.Contains("1.5")) modelId = "gemini-2.5-flash"; // Auto-upgrade old settings
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

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException("Audio file not found at " + audioFilePath);
        }

        var settings = await GetSettingsAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.GcpProjectId))
        {
            throw new InvalidOperationException("Vertex AI settings are not fully configured. Please configure your GCP Project ID in Settings.");
        }

        var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
        if (token == null)
        {
            throw new InvalidOperationException("Failed to obtain GCP access token. Please check your credentials file path or ensure Application Default Credentials are set up.");
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(audioFilePath);
            var base64Data = Convert.ToBase64String(bytes);

            string extension = Path.GetExtension(audioFilePath).ToLower();
            string mimeType = extension switch
            {
                ".wav" => "audio/wav",
                ".mp3" => "audio/mp3",
                ".m4a" => "audio/m4a",
                ".ogg" => "audio/ogg",
                ".flac" => "audio/flac",
                _ => "audio/wav"
            };

            string projectId = settings.GcpProjectId;
            string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
            string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash" : settings.PreferredModel;

            if (modelId.Contains("1.5")) modelId = "gemini-2.5-flash"; // Auto-upgrade old settings
            if (modelId == "Gemini 2.5 Flash") modelId = "gemini-2.5-flash";
            if (modelId == "Gemini 2.5 Pro") modelId = "gemini-2.5-pro";

            string endpoint = $"https://{region}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{region}/publishers/google/models/{modelId}:generateContent";

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "You are a precise audio transcription system. Your task is to transcribe the audio content exactly as spoken. Do not add any introduction, explanations, meta-comments, or corrections. Output ONLY the transcribed text." } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { inlineData = new { mimeType = mimeType, data = base64Data } },
                            new { text = "Transcribe this audio file." }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini Transcription API error: {response.StatusCode} - {err}");
                throw new InvalidOperationException($"Transcription request failed: {(int)response.StatusCode} - {response.ReasonPhrase}. Details: {err}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonResponse);

            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            return text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio.");
            throw;
        }
    }

    public async Task<SpeakingGradingResult> GradeSpeakingAsync(string transcript, int partNumber, string? audioFilePath, int wpmRate = 0, int longPauseCount = 0)
    {
        _logger.LogInformation($"Grading speaking for Part {partNumber}...");

        string systemPrompt =
            "You are an expert, un-biased IELTS Examiner engine. Your job is to analyze student transcripts and optional spoken audio and output rigorous, evidence-based IELTS band scores from 0.0 to 9.0 based strictly on the official public band descriptors.\n\n" +
            "Execute the evaluation in two distinct passes:\n\n" +
            "PASS 1: The Analytical Audit (JSON Only)\n" +
            "Assess each core criterion independently according to these rules:\n" +
            "- Fluency & Coherence (FC):\n" +
            "  - IF the speaker shows long pauses mid-sentence strictly to hunt for vocabulary or grammar rules (language-related hesitation): MAX CAP: 6.0 for FC.\n" +
            "  - IF hesitation is purely abstract or content-related (thinking of WHAT to say, not HOW to say it) and they can speak at length without effort: SET TO: 7.0+ for FC.\n" +
            "  - IF they constantly overuse repetitive, basic discourse markers (\"and then...\", \"you know...\", \"actually\") mechanically: MAX CAP: 6.0 for FC.\n" +
            "  - Pauses > 2.0s indicating search for language drop the score below Band 7.0.\n" +
            "- Lexical Resource (LR):\n" +
            "  - IF the speaker only uses simple vocabulary and completely fails when attempting to paraphrase an unknown word: MAX CAP: 5.0 for LR.\n" +
            "  - IF they paraphrase successfully overall but lack any less common idiomatic phrases or natural collocations: MAX CAP: 6.0 for LR.\n" +
            "  - IF they smoothly deploy idiomatic items and show stylistic awareness, even with occasional inappropriate choices: SET TO: 7.0+ for LR.\n" +
            "- Grammatical Range and Accuracy (GRA):\n" +
            "  - IF they rely almost entirely on simple sentence structures and their attempts at complex clauses cause frequent disfluency: MAX CAP: 5.0 for GRA.\n" +
            "  - IF they use a mix of simple and complex sentence forms but make frequent systematic errors in complex structures: MAX CAP: 6.0 for GRA.\n" +
            "  - IF they frequently produce error-free sentences and handle a range of complex structural flexibility: SET TO: 7.0+ for GRA.\n" +
            "- Pronunciation (PR):\n" +
            "  - IF mispronunciations of individual words or sounds frequently reduce clarity or force the listener to strain: MAX CAP: 6.0 for PR.\n" +
            "  - IF they demonstrate effective chunking, clear sentence/word stress, and overall high intelligibility throughout, despite a noticeable first-language accent: SET TO: 7.0+ for PR.\n\n" +
            "For every single criterion, you must populate these exact keys inside the \"analytical_criteria_scores\" object:\n" +
            "1. \"band\": An individual score formatted as a float (e.g., 6.0, 7.0).\n" +
            "2. \"key_justification\": A concise textual explanation of the score.\n" +
            "3. \"supporting_evidence_quotes\": An array of exact, unedited strings from the student's transcript.\n" +
            "4. \"limiting_factors\": An array of specific structural rules that capped or decided the score.\n" +
            "5. \"matched_descriptor_ids\": An array of specific string IDs from the provided rubric JSON that the student achieved.\n\n" +
            "Speaking Module Telemetry:\n" +
            "Map the provided part-specific telemetry inputs (wpm_rate, long_pause_count) directly to their respective parts ('part_1', 'part_2', 'part_3') within the 'test_parts_breakdown' attribute of the output JSON.\n\n" +
            "Core Band Calculation Algorithmic Rules:\n" +
            "Calculate the arithmetic mean of the 4 analytical criteria scores, then determine the final overall band using these strict mathematical boundaries:\n" +
            "- If decimal fraction is < 0.25 -> Round DOWN to .0 (e.g., 6.125 -> 6.0)\n" +
            "- If decimal fraction is >= 0.25 and < 0.75 -> Round to .5 (e.g., 6.25 -> 6.5, 6.625 -> 6.5)\n" +
            "- If decimal fraction is >= 0.75 -> Round UP to the next whole integer .0 (e.g., 6.75 -> 7.0)\n" +
            "Assign this calculated result to \"overall_calculated_band\" root key.\n\n" +
            "PASS 2: Pedagogical Coaching (Prose)\n" +
            "Convert the raw data from Pass 1 into constructive, actionable coaching advice. Populate the \"student_coaching\" object precisely using these keys:\n" +
            "1. \"core_strengths\": Highlight what the student did well structurally.\n" +
            "2. \"primary_weakness_to_fix\": Pinpoint the single most critical grammar structure or vocabulary cluster the student needs to fix to unlock the next band level.\n" +
            "3. \"actionable_practice_exercise\": Provide a highly tailored short prompt or drill targeting that exact weakness.\n\n" +
            "Never let feedback alter the calculated scores from Pass 1.\n\n" +
            "Return ONLY a JSON object matching this schema:\n" +
            "{\n" +
            "  \"overall_calculated_band\": 0.0,\n" +
            "  \"test_parts_breakdown\": {\n" +
            "    \"part_1\": { \"wpm_rate\": 0, \"part_band_estimate\": 0.0 },\n" +
            "    \"part_2\": { \"wpm_rate\": 0, \"long_pause_count\": 0, \"part_band_estimate\": 0.0 },\n" +
            "    \"part_3\": { \"wpm_rate\": 0, \"part_band_estimate\": 0.0 }\n" +
            "  },\n" +
            "  \"analytical_criteria_scores\": {\n" +
            "    \"fluency_coherence\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"],\n" +
            "      \"matched_descriptor_ids\": [\"string\"]\n" +
            "    },\n" +
            "    \"lexical_resource\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"],\n" +
            "      \"matched_descriptor_ids\": [\"string\"]\n" +
            "    },\n" +
            "    \"grammatical_range_accuracy\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"],\n" +
            "      \"matched_descriptor_ids\": [\"string\"]\n" +
            "    },\n" +
            "    \"pronunciation\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"],\n" +
            "      \"matched_descriptor_ids\": [\"string\"]\n" +
            "    }\n" +
            "  },\n" +
            "  \"student_coaching\": {\n" +
            "    \"core_strengths\": \"string\",\n" +
            "    \"primary_weakness_to_fix\": \"string\",\n" +
            "    \"actionable_practice_exercise\": \"string\"\n" +
            "  }\n" +
            "}\n\n" +
            $"Official Speaking Rubric:\n{JsonSerializer.Serialize(IeltsDescriptors.SpeakingDescriptors, new JsonSerializerOptions { WriteIndented = true })}";

        string userPromptText = $"Evaluate Part {partNumber} of IELTS Speaking.\n" +
            $"Telemetry for Part {partNumber}:\n" +
            $"- Words Per Minute (wpm_rate): {wpmRate}\n" +
            $"- Long Pause Count: {longPauseCount}\n\n" +
            $"Transcript text:\n{transcript}";

        string? resultJson = null;

        // Try to load settings and run multimodal evaluation if audio file exists
        bool useAudio = !string.IsNullOrEmpty(audioFilePath) && File.Exists(audioFilePath);
        if (useAudio)
        {
            var settings = await GetSettingsAsync();
            if (settings != null && !string.IsNullOrWhiteSpace(settings.GcpProjectId))
            {
                var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
                if (token != null)
                {
                    try
                    {
                        var bytes = await File.ReadAllBytesAsync(audioFilePath!);
                        var base64Data = Convert.ToBase64String(bytes);

                        string extension = Path.GetExtension(audioFilePath!).ToLower();
                        string mimeType = extension switch
                        {
                            ".wav" => "audio/wav",
                            ".mp3" => "audio/mp3",
                            ".m4a" => "audio/m4a",
                            ".ogg" => "audio/ogg",
                            ".flac" => "audio/flac",
                            _ => "audio/wav"
                        };

                        string projectId = settings.GcpProjectId;
                        string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
                        string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash" : settings.PreferredModel;

                        if (modelId.Contains("1.5")) modelId = "gemini-2.5-flash"; // Auto-upgrade old settings
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
                                new
                                {
                                    role = "user",
                                    parts = new object[]
                                    {
                                        new { inlineData = new { mimeType = mimeType, data = base64Data } },
                                        new { text = $"Please evaluate the provided speaking audio performance alongside its transcript text.\n{userPromptText}" }
                                    }
                                }
                            },
                            generationConfig = new { responseMimeType = "application/json" }
                        };

                        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                        var client = _httpClientFactory.CreateClient();
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            using var document = JsonDocument.Parse(jsonResponse);
                            resultJson = document.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text").GetString();
                        }
                        else
                        {
                            string err = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning($"Multimodal grading failed, status code: {response.StatusCode} - {err}. Falling back to text-only grading.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to run multimodal evaluation. Falling back to text-only grading.");
                    }
                }
            }
        }

        // Fallback to text-only grading if multimodal grading wasn't successful or audio wasn't provided
        if (resultJson == null)
        {
            if (useAudio)
            {
                systemPrompt += "\nNote: There was an issue processing the audio file. Please grade based on the transcript and add a note about this fallback.";
            }
            resultJson = await CallGeminiAsync(systemPrompt, userPromptText);
        }

        if (resultJson != null)
        {
            try
            {
                int start = resultJson.IndexOf('{');
                int end = resultJson.LastIndexOf('}');
                if (start >= 0 && end >= start)
                {
                    resultJson = resultJson.Substring(start, end - start + 1);
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
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

        string systemPrompt =
            "You are an expert, un-biased IELTS Examiner engine. Your job is to analyze student essays and output rigorous, evidence-based IELTS band scores from 0.0 to 9.0 based strictly on the official public band descriptors.\n\n" +
            "Execute the evaluation in two distinct passes:\n\n" +
            "PASS 1: The Analytical Audit (JSON Only)\n" +
            "Assess each core criterion independently according to these rules:\n" +
            "- Task Achievement / Task Response (TR):\n" +
            "  - IF the student fails to outline a clear position in the introduction OR switches side at the end: MAX CAP: 6.0 for TR.\n" +
            "  - IF any main parts of the prompt are ignored (e.g., discusses advantages but forgets disadvantages): MAX CAP: 5.0 for TR.\n" +
            "  - IF all parts are addressed but ideas are generic or lack deeper extension/examples: SET TO: 6.0 or 7.0 based on clarity.\n" +
            "- Coherence and Cohesion (CC):\n" +
            "  - IF every single sentence starts with a repetitive mechanical linker (e.g., \"Furthermore,\", \"Moreover,\", \"In addition,\"): MAX CAP: 6.0 for CC.\n" +
            "  - IF referencing pronouns (e.g., \"this trend\", \"these factors\", \"it\") are missing or cause confusion: MAX CAP: 6.0 for CC.\n" +
            "  - IF paragraphs are present but lack a clear central topic sentence: MAX CAP: 6.0 for CC.\n" +
            "- Lexical Resource (LR):\n" +
            "  - IF spelling/word-formation errors occur frequently enough to make the reader pause or guess the meaning: MAX CAP: 5.0 for LR.\n" +
            "  - IF vocabulary is accurate but entirely simple and repetitive without any collocations or stylistic awareness: MAX CAP: 6.0 for LR.\n" +
            "- Grammatical Range and Accuracy (GRA):\n" +
            "  - IF simple sentences dominate and subordinate/complex clauses (conditionals, relative clauses) are rare or broken: MAX CAP: 5.0 for GRA.\n" +
            "  - IF errors are present in complex structures but the majority of basic sentences remain completely error-free: SET TO: 7.0 for GRA.\n\n" +
            "For every single criterion, you must populate these exact keys inside the \"analytical_criteria_scores\" object:\n" +
            "1. \"task_response\" (for both Task 1 and Task 2, map it here):\n" +
            "   - \"band\": An individual score formatted as a float (e.g., 6.0, 7.0).\n" +
            "   - \"key_justification\": A concise textual explanation of the score.\n" +
            "   - \"supporting_evidence_quotes\": An array of exact, unedited strings from the student's submission.\n" +
            "   - \"limiting_factors\": An array of specific structural rules that capped or decided the score.\n" +
            "   - \"matched_descriptor_ids\": An array of string IDs from the provided rubric JSON that the student achieved.\n" +
            "2. \"coherence_cohesion\": (same structure)\n" +
            "3. \"lexical_resource\": (same structure)\n" +
            "4. \"grammatical_range_accuracy\": (same structure)\n\n" +
            "Writing Module Prompt Capture:\n" +
            "You MUST capture the input question prompt text and assign it verbatim to the \"essay_prompt_provided\" root key.\n\n" +
            "Core Band Calculation Algorithmic Rules:\n" +
            "Calculate the arithmetic mean of the 4 analytical criteria scores, then determine the final overall band using these strict mathematical boundaries:\n" +
            "- If decimal fraction is < 0.25 -> Round DOWN to .0 (e.g., 6.125 -> 6.0)\n" +
            "- If decimal fraction is >= 0.25 and < 0.75 -> Round to .5 (e.g., 6.25 -> 6.5, 6.625 -> 6.5)\n" +
            "- If decimal fraction is >= 0.75 -> Round UP to the next whole integer .0 (e.g., 6.75 -> 7.0)\n" +
            "Assign this calculated result to \"calculated_overall_writing_band\" root key.\n\n" +
            "PASS 2: Pedagogical Coaching (Prose)\n" +
            "Convert the raw data from Pass 1 into constructive, actionable coaching advice. Populate the \"student_coaching\" object precisely using these keys:\n" +
            "1. \"core_strengths\": Highlight what the student did well structurally.\n" +
            "2. \"primary_weakness_to_fix\": Pinpoint the single most critical grammar structure or vocabulary cluster the student needs to fix to unlock the next band level.\n" +
            "3. \"actionable_practice_exercise\": Provide a highly tailored short prompt or drill targeting that exact weakness.\n\n" +
            "Never let feedback alter the calculated scores from Pass 1.\n\n" +
            "Mandatory Scoring Boundary Controls:\n" +
            "- band 7 TR requires: Addressing ALL parts of the prompt with a clear position throughout.\n" +
            "- band 7 CC requires: A clear progression of ideas and non-repetitive cohesive devices.\n" +
            "- band 7 LR requires: Attempted use of less common vocabulary and collocations with stylistic awareness.\n" +
            "- band 7 GRA requires: Frequent error-free sentences containing complex structural variety.\n\n" +
            "Return ONLY a JSON object matching this schema:\n" +
            "{\n" +
            "  \"essay_prompt_provided\": \"string\",\n" +
            "  \"calculated_overall_writing_band\": 0.0,\n" +
            "  \"analytical_criteria_scores\": {\n" +
            "    \"task_response\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"]\n" +
            "    },\n" +
            "    \"coherence_cohesion\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"]\n" +
            "    },\n" +
            "    \"lexical_resource\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"]\n" +
            "    },\n" +
            "    \"grammatical_range_accuracy\": {\n" +
            "      \"band\": 0.0,\n" +
            "      \"key_justification\": \"string\",\n" +
            "      \"supporting_evidence_quotes\": [\"string\"],\n" +
            "      \"limiting_factors\": [\"string\"]\n" +
            "    }\n" +
            "  },\n" +
            "  \"student_coaching\": {\n" +
            "    \"core_strengths\": \"string\",\n" +
            "    \"primary_weakness_to_fix\": \"string\",\n" +
            "    \"actionable_practice_exercise\": \"string\"\n" +
            "  }\n" +
            "}\n\n" +
            $"Official Writing Rubric:\n{JsonSerializer.Serialize(IeltsDescriptors.WritingDescriptors, new JsonSerializerOptions { WriteIndented = true })}";

        string userPrompt = $"Evaluate Task {taskNumber} of IELTS Writing.\n" +
            $"Prompt:\n{promptText}\n\nStudent Writing:\n{text}";

        string? resultJson = await CallGeminiAsync(systemPrompt, userPrompt);

        if (resultJson != null)
        {
            try
            {
                int start = resultJson.IndexOf('{');
                int end = resultJson.LastIndexOf('}');
                if (start >= 0 && end >= start)
                {
                    resultJson = resultJson.Substring(start, end - start + 1);
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
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

    public async Task<string> ExtractTextFromPdfOrImageAsync(string filePath)
    {
        _logger.LogInformation($"Extracting text from document: {filePath}");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found at " + filePath);
        }

        var settings = await GetSettingsAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.GcpProjectId))
        {
            throw new InvalidOperationException("Vertex AI settings are not fully configured. Please configure your GCP Project ID in Settings.");
        }

        var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
        if (token == null)
        {
            throw new InvalidOperationException("Failed to obtain GCP access token. Please check your credentials file path or ensure Application Default Credentials are set up.");
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            var base64Data = Convert.ToBase64String(bytes);

            string extension = Path.GetExtension(filePath).ToLower();
            string mimeType = extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => throw new NotSupportedException($"File extension {extension} is not supported for extraction.")
            };

            string projectId = settings.GcpProjectId;
            string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
            string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash" : settings.PreferredModel;

            if (modelId.Contains("1.5")) modelId = "gemini-2.5-flash"; // Auto-upgrade old settings
            if (modelId == "Gemini 2.5 Flash") modelId = "gemini-2.5-flash";
            if (modelId == "Gemini 2.5 Pro") modelId = "gemini-2.5-pro";

            string endpoint = $"https://{region}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{region}/publishers/google/models/{modelId}:generateContent";

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "You are a precise document text extraction system. Your task is to extract all user-written text or essay text from the document. Do not include any instructions, page numbers, or questions if they are part of a template. Only return the actual student essay or writing. Do not format the output. Output ONLY the raw extracted text." } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { inlineData = new { mimeType = mimeType, data = base64Data } },
                            new { text = "Extract all text from this document." }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini Extraction API error: {response.StatusCode} - {err}");
                throw new InvalidOperationException($"Extraction request failed: {(int)response.StatusCode} - {response.ReasonPhrase}. Details: {err}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonResponse);

            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            return text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from document.");
            throw;
        }
    }

    private async Task<string?> CallGeminiTextAsync(string systemPrompt, string userPrompt)
    {
        var settings = await GetSettingsAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.GcpProjectId))
        {
            _logger.LogWarning("Vertex AI settings are not fully configured. Project ID is missing.");
            return null;
        }

        var token = await GetAccessTokenAsync(settings.GcpCredentialsPath);
        if (token == null) return null;

        string projectId = settings.GcpProjectId;
        string region = string.IsNullOrWhiteSpace(settings.GcpRegion) ? "us-central1" : settings.GcpRegion;
        string modelId = string.IsNullOrWhiteSpace(settings.PreferredModel) ? "gemini-2.5-flash" : settings.PreferredModel;

        if (modelId.Contains("1.5")) modelId = "gemini-2.5-flash"; // Auto-upgrade old settings
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
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string err = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Gemini Text API error: {response.StatusCode} - {err}");
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
            _logger.LogError(ex, "Failed to parse Gemini text response.");
            return null;
        }
    }

    public async Task<string> AnalyzeReadingAsync(string passage, string studentAnswers)
    {
        _logger.LogInformation("Analyzing reading misunderstanding...");
        string systemPrompt =
            "You are an expert IELTS Reading tutor. Analyze the provided Reading Passage and the Student's Answers. Highlight where the student made mistakes (showing their answer vs correct answer) and map out the specific potential misunderstanding or cognitive trap they fell into (e.g. 'Misinterpreting distractors', 'Vocabulary mismatch in paraphrase', 'Over-generalization', 'Failing to locate detail'). Output your analysis in a clean, professional, highly readable markdown format with headings, bullet points, and highlight blocks.";

        string userPrompt = $"Reading Passage:\n{passage}\n\nStudent's Answers:\n{studentAnswers}";
        var result = await CallGeminiTextAsync(systemPrompt, userPrompt);
        return result ?? "Error: Unable to analyze reading answers at this time.";
    }

    public async Task<string> AnalyzeListeningAsync(string script, string studentAnswers)
    {
        _logger.LogInformation("Analyzing listening auditory and spelling...");
        string systemPrompt =
            "You are an expert IELTS Listening tutor. Analyze the provided Listening Script/Context and the Student's Answers. Pinpoint wrong answers (student's answer vs correct answer) and output the potential reasons for error (e.g. 'Connected speech omission', 'Homophone error', 'Failure to identify signposting words', 'Spelling mismatch', 'Plural/singular confusion'). Output your analysis in a clean, professional, highly readable markdown format with headings, bullet points, and highlight blocks.";

        string userPrompt = $"Listening Script/Context:\n{script}\n\nStudent's Answers:\n{studentAnswers}";
        var result = await CallGeminiTextAsync(systemPrompt, userPrompt);
        return result ?? "Error: Unable to analyze listening answers at this time.";
    }
}

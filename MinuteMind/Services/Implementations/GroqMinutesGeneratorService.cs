using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class GroqMinutesGeneratorService(IHttpClientFactory httpClientFactory) : IMinutesGeneratorService
{
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "llama-3.3-70b-versatile";
    private const int MaxTranscriptChars = 8000;

    private static readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private static readonly string _systemPrompt =
        "You are a meeting minutes assistant. Extract structured notes from the meeting transcript.\n" +
        "Return ONLY a valid JSON object with this exact schema:\n" +
        "{\n" +
        "  \"Attendees\": [\"name or Speaker 1 if unknown\"],\n" +
        "  \"DiscussionPoints\": [\"concise bullet point\"],\n" +
        "  \"Decisions\": [{\"Category\": \"Technical|Process|Budget|Other\", \"Text\": \"decision made\"}],\n" +
        "  \"ActionItems\": [{\"Description\": \"task\", \"Assignee\": \"person or Unknown\", \"DueDate\": \"date or empty string\", \"IsCompleted\": false}]\n" +
        "}";

    public async Task<MeetingMinutes> GenerateAsync(
        List<TranscriptSegment> transcript,
        IProgress<string>? progress = null)
    {
        var apiKey = Preferences.Default.Get("groq_api_key", string.Empty);
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "No Groq API key set. Open Settings and paste your free key from console.groq.com.");

        progress?.Report("Sending to Groq AI…");

        var requestBody = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "system", content = _systemPrompt },
                new { role = "user",   content = BuildTranscriptText(transcript) }
            },
            temperature = 0.1,
            max_tokens = 1024,
            response_format = new { type = "json_object" }
        };

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new StringContent(
            JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(ApiUrl, body);
        response.EnsureSuccessStatusCode();

        progress?.Report("Parsing minutes…");

        var json = await response.Content.ReadAsStringAsync();
        return ParseResponse(json);
    }

    private static string BuildTranscriptText(List<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        foreach (var seg in segments)
        {
            var line = $"[{seg.TimestampDisplay}] {seg.Speaker}: {seg.Text}\n";
            if (sb.Length + line.Length > MaxTranscriptChars) break;
            sb.Append(line);
        }
        return sb.ToString();
    }

    private static MeetingMinutes ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (content is not null)
            {
                var result = JsonSerializer.Deserialize<MeetingMinutes>(content, _jsonOptions);
                if (result is not null) return result;
            }
        }
        catch { }

        return new MeetingMinutes
        {
            DiscussionPoints = ["Could not parse AI response — please edit manually."]
        };
    }
}

using System.Text;
using System.Text.Json;
using LLama;
using LLama.Common;
using LLama.Sampling;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class LlmMinutesGeneratorService(IHttpClientFactory httpClientFactory) : IMinutesGeneratorService
{
    private const string ModelFileName = "qwen2.5-0.5b-instruct-q4_k_m.gguf";
    private const string ModelUrl = "https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF/resolve/main/qwen2.5-0.5b-instruct-q4_k_m.gguf";
    private const int MaxTranscriptChars = 6000;

    private string ModelPath => Path.Combine(FileSystem.AppDataDirectory, ModelFileName);

    public async Task<MeetingMinutes> GenerateAsync(
        List<TranscriptSegment> transcript,
        IProgress<string>? progress = null)
    {
        await EnsureModelDownloadedAsync(progress);

        progress?.Report("Loading AI model…");
        var modelParams = new ModelParams(ModelPath)
        {
            ContextSize = 4096u,
            GpuLayerCount = 0
        };

        using var weights = LLamaWeights.LoadFromFile(modelParams);
        var executor = new StatelessExecutor(weights, modelParams, logger: null);

        progress?.Report("Generating minutes…");
        var prompt = BuildPrompt(transcript);
        var inferenceParams = new InferenceParams
        {
            MaxTokens = 1024,
            AntiPrompts = ["<|im_end|>", "<|endoftext|>", "<|im_start|>"],
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.1f
            }
        };

        var sb = new StringBuilder();
        await foreach (var token in executor.InferAsync(prompt, inferenceParams))
        {
            sb.Append(token);
        }

        return ParseMinutes(sb.ToString());
    }

    private async Task EnsureModelDownloadedAsync(IProgress<string>? progress)
    {
        if (File.Exists(ModelPath))
            return;

        var tempPath = ModelPath + ".tmp";
        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var file = File.Create(tempPath);

            var buffer = new byte[81920];
            long downloaded = 0;
            int read;
            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, read));
                downloaded += read;
                if (totalBytes > 0)
                {
                    var pct = (int)(downloaded * 100 / totalBytes);
                    progress?.Report($"Downloading AI model ({pct}%)…");
                }
            }
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }

        File.Move(tempPath, ModelPath);
    }

    private static string BuildPrompt(List<TranscriptSegment> transcript)
    {
        const string system =
            "You are a meeting minutes assistant. Extract structured notes from the meeting transcript below.\n" +
            "Return ONLY a valid JSON object — no explanation, no markdown, no extra text.\n" +
            "Use this exact schema:\n" +
            "{\n" +
            "  \"Attendees\": [\"name or Speaker 1 if unknown\"],\n" +
            "  \"DiscussionPoints\": [\"concise bullet point\"],\n" +
            "  \"Decisions\": [{\"Category\": \"Technical|Process|Budget|Other\", \"Text\": \"decision made\"}],\n" +
            "  \"ActionItems\": [{\"Description\": \"task\", \"Assignee\": \"person or Unknown\", \"DueDate\": \"date or empty string\", \"IsCompleted\": false}]\n" +
            "}";

        var transcriptText = new StringBuilder();
        foreach (var seg in transcript)
        {
            var line = $"[{seg.Timestamp:mm\\:ss}] {seg.Speaker}: {seg.Text}\n";
            if (transcriptText.Length + line.Length > MaxTranscriptChars)
                break;
            transcriptText.Append(line);
        }

        return
            "<|im_start|>system\n" + system + "\n<|im_end|>\n" +
            "<|im_start|>user\n" + transcriptText + "<|im_end|>\n" +
            "<|im_start|>assistant\n";
    }

    private static MeetingMinutes ParseMinutes(string output)
    {
        try
        {
            var start = output.IndexOf('{');
            var end = output.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                var json = output[start..(end + 1)];
                var result = JsonSerializer.Deserialize<MeetingMinutes>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result is not null)
                    return result;
            }
        }
        catch
        {
            // fall through to fallback
        }

        return new MeetingMinutes
        {
            DiscussionPoints = ["Could not parse AI output — please edit manually."]
        };
    }
}

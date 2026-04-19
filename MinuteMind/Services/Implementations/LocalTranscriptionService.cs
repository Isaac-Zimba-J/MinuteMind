using MinuteMind.Models;
using MinuteMind.Services.Contracts;
using Whisper.net;
using Whisper.net.Ggml;

namespace MinuteMind.Services.Implementations;

public class LocalTranscriptionService : ITranscriptionService
{
    private string? _modelPath;

    private async Task<string> EnsureModelAsync(IProgress<string>? progress)
    {
        if (_modelPath is not null && File.Exists(_modelPath))
            return _modelPath;

        var dest = Path.Combine(FileSystem.AppDataDirectory, "ggml-tiny.bin");

        if (!File.Exists(dest))
        {
            progress?.Report("Copying model to device...");
            using var src = await FileSystem.OpenAppPackageFileAsync("ggml-tiny.bin");
            using var file = File.Create(dest);
            await src.CopyToAsync(file);
        }

        _modelPath = dest;
        return dest;
    }

    public async Task<List<TranscriptSegment>> TranscribeAsync(string audioPath, IProgress<string>? progress = null)
    {
        var modelPath = await EnsureModelAsync(progress);

        progress?.Report("Loading model...");

        using var factory = WhisperFactory.FromPath(modelPath);
        using var processor = factory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        progress?.Report("Transcribing audio...");

        var segments = new List<TranscriptSegment>();

        await using var fileStream = File.OpenRead(audioPath);

        await foreach (var segment in processor.ProcessAsync(fileStream))
        {
            segments.Add(new TranscriptSegment
            {
                Timestamp = segment.Start,
                Speaker = "Speaker 1",
                Text = segment.Text.Trim()
            });

            progress?.Report($"Transcribed {segment.End:mm\\:ss}...");
        }

        return segments;
    }
}

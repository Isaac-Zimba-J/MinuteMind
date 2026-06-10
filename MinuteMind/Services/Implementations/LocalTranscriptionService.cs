using MinuteMind.Models;
using MinuteMind.Services.Contracts;
using Whisper.net;
using Whisper.net.Ggml;

namespace MinuteMind.Services.Implementations;

public class LocalTranscriptionService : ITranscriptionService
{
    private const int WhisperSampleRate = 16000;

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
            .WithLanguage(GetWhisperLanguageCode())
            .WithPrompt("Meeting discussion, business context, action items, decisions:")
            .Build();

        progress?.Report("Preparing audio...");
        var processPath = await EnsureWhisperFormatAsync(audioPath);

        progress?.Report("Transcribing audio...");

        var segments = new List<TranscriptSegment>();

        try
        {
            await using var fileStream = File.OpenRead(processPath);

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
        }
        finally
        {
            if (processPath != audioPath && File.Exists(processPath))
                File.Delete(processPath);
        }

        return segments;
    }

    // Converts any WAV to 16kHz mono 16-bit PCM required by Whisper.
    // Returns the original path unchanged if it already matches.
    private static Task<string> EnsureWhisperFormatAsync(string inputPath)
    {
        using var input = File.OpenRead(inputPath);
        using var reader = new BinaryReader(input);

        // Parse RIFF/WAVE header
        if (new string(reader.ReadChars(4)) != "RIFF") return Task.FromResult(inputPath);
        reader.ReadInt32(); // file size
        if (new string(reader.ReadChars(4)) != "WAVE") return Task.FromResult(inputPath);

        int srcSampleRate = 0, srcChannels = 0, srcBitsPerSample = 0;
        byte[]? pcmData = null;

        while (input.Position < input.Length - 8)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            var chunkStart = input.Position;

            if (chunkId == "fmt ")
            {
                reader.ReadInt16(); // audio format
                srcChannels = reader.ReadInt16();
                srcSampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadInt16(); // block align
                srcBitsPerSample = reader.ReadInt16();
            }
            else if (chunkId == "data")
            {
                pcmData = reader.ReadBytes(chunkSize);
            }

            // Seek to next chunk (handles extra fmt bytes, odd sizes, etc.)
            input.Position = chunkStart + chunkSize + (chunkSize % 2); // RIFF pads odd chunks
        }

        if (pcmData is null || srcSampleRate == 0)
            return Task.FromResult(inputPath);

        if (srcSampleRate == WhisperSampleRate && srcChannels == 1 && srcBitsPerSample == 16)
            return Task.FromResult(inputPath);

        // Decode interleaved PCM to mono float samples
        int bytesPerSample = srcBitsPerSample / 8;
        int totalSamples = pcmData.Length / bytesPerSample / srcChannels;
        var monoFloat = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float sum = 0f;
            for (int ch = 0; ch < srcChannels; ch++)
            {
                int offset = (i * srcChannels + ch) * bytesPerSample;
                sum += srcBitsPerSample switch
                {
                    16 => BitConverter.ToInt16(pcmData, offset) / 32768f,
                    8  => (pcmData[offset] - 128) / 128f,
                    32 => BitConverter.ToInt32(pcmData, offset) / 2147483648f,
                    _  => 0f
                };
            }
            monoFloat[i] = sum / srcChannels;
        }

        // Linear-interpolation resample to WhisperSampleRate
        double ratio = (double)srcSampleRate / WhisperSampleRate;
        int dstCount = (int)(totalSamples / ratio);
        var resampled = new short[dstCount];

        for (int i = 0; i < dstCount; i++)
        {
            double pos = i * ratio;
            int idx = (int)pos;
            double frac = pos - idx;
            float s0 = idx < monoFloat.Length ? monoFloat[idx] : 0f;
            float s1 = idx + 1 < monoFloat.Length ? monoFloat[idx + 1] : 0f;
            resampled[i] = (short)Math.Clamp((s0 + frac * (s1 - s0)) * 32767, -32768, 32767);
        }

        // Write 16kHz mono 16-bit WAV
        var outputPath = Path.ChangeExtension(inputPath, null) + "_16k.wav";
        int dataBytes = dstCount * 2;

        using var output = File.Create(outputPath);
        using var writer = new BinaryWriter(output);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);            // PCM
        writer.Write((short)1);            // mono
        writer.Write(WhisperSampleRate);
        writer.Write(WhisperSampleRate * 2); // byte rate
        writer.Write((short)2);            // block align
        writer.Write((short)16);           // bits per sample
        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes);
        foreach (var s in resampled) writer.Write(s);

        return Task.FromResult(outputPath);
    }

    private static string GetWhisperLanguageCode() =>
        Preferences.Default.Get("settings_language", "English") switch
        {
            "English"    => "en",
            "Spanish"    => "es",
            "French"     => "fr",
            "German"     => "de",
            "Portuguese" => "pt",
            "Italian"    => "it",
            "Chinese"    => "zh",
            "Japanese"   => "ja",
            "Korean"     => "ko",
            "Arabic"     => "ar",
            "Hindi"      => "hi",
            _            => "auto"
        };
}

using MinuteMind.Models;
using MinuteMind.Services.Contracts;
using Whisper.net;

namespace MinuteMind.Services.Implementations;

public class LocalTranscriptionService : ITranscriptionService
{
    private const int WhisperSampleRate = 16000;
    private readonly IAudioConverter _converter;
    private string? _modelPath;

    public LocalTranscriptionService(IAudioConverter converter)
    {
        _converter = converter;
    }

    public async Task<List<TranscriptSegment>> TranscribeAsync(
        string audioPath, IProgress<string>? progress = null)
    {
        var modelPath = await EnsureModelAsync(progress);

        progress?.Report("Loading model...");

        using var factory   = WhisperFactory.FromPath(modelPath);
        using var processor = factory.CreateBuilder()
            .WithLanguage(GetLanguageCode())
            .WithPrompt("Meeting discussion, business context, action items, decisions:")
            .Build();

        progress?.Report("Preparing audio...");
        var processPath = await EnsureWhisperFormatAsync(audioPath, progress);

        progress?.Report("Transcribing audio...");
        var segments = new List<TranscriptSegment>();

        try
        {
            await using var fs = File.OpenRead(processPath);
            await foreach (var seg in processor.ProcessAsync(fs))
            {
                segments.Add(new TranscriptSegment
                {
                    Timestamp = seg.Start,
                    Speaker   = "Speaker 1",
                    Text      = seg.Text.Trim()
                });
                progress?.Report($"Transcribed {seg.End:mm\\:ss}...");
            }
        }
        finally
        {
            if (processPath != audioPath && File.Exists(processPath))
                File.Delete(processPath);
        }

        return segments;
    }

    // Routes non-WAV files through the platform converter; resamples WAV files to
    // 16 kHz / mono / 16-bit as required by Whisper.
    private async Task<string> EnsureWhisperFormatAsync(
        string inputPath, IProgress<string>? progress)
    {
        bool isWav = false;
        try
        {
            using var probe = File.OpenRead(inputPath);
            var hdr = new byte[4];
            if (probe.Read(hdr, 0, 4) == 4)
                isWav = hdr[0] == 'R' && hdr[1] == 'I' && hdr[2] == 'F' && hdr[3] == 'F';
        }
        catch { }

        if (!isWav)
            return await _converter.ConvertToWavAsync(inputPath, progress);

        return await Task.Run(() => ReformatWavSync(inputPath));
    }

    // Resamples an in-spec WAV to 16 kHz / mono / 16-bit.
    private static string ReformatWavSync(string inputPath)
    {
        using var input  = File.OpenRead(inputPath);
        using var reader = new BinaryReader(input);

        if (new string(reader.ReadChars(4)) != "RIFF") return inputPath;
        reader.ReadInt32();
        if (new string(reader.ReadChars(4)) != "WAVE") return inputPath;

        int srcRate = 0, srcChans = 0, srcBits = 0;
        byte[]? pcmData = null;

        while (input.Position < input.Length - 8)
        {
            var chunkId   = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            var chunkPos  = input.Position;

            if (chunkId == "fmt ")
            {
                reader.ReadInt16();
                srcChans = reader.ReadInt16();
                srcRate  = reader.ReadInt32();
                reader.ReadInt32(); reader.ReadInt16();
                srcBits = reader.ReadInt16();
            }
            else if (chunkId == "data")
            {
                pcmData = reader.ReadBytes(chunkSize);
            }

            input.Position = chunkPos + chunkSize + (chunkSize % 2);
        }

        if (pcmData is null || srcRate == 0) return inputPath;
        if (srcRate == WhisperSampleRate && srcChans == 1 && srcBits == 16)
            return inputPath;

        int bps    = srcBits / 8;
        int total  = pcmData.Length / bps / srcChans;
        var mono   = new float[total];

        for (int i = 0; i < total; i++)
        {
            float s = 0f;
            for (int ch = 0; ch < srcChans; ch++)
            {
                int o = (i * srcChans + ch) * bps;
                s += srcBits switch
                {
                    16 => BitConverter.ToInt16(pcmData, o) / 32768f,
                    8  => (pcmData[o] - 128) / 128f,
                    32 => BitConverter.ToInt32(pcmData, o) / 2147483648f,
                    _  => 0f
                };
            }
            mono[i] = s / srcChans;
        }

        double ratio = (double)srcRate / WhisperSampleRate;
        int    dstN  = (int)(total / ratio);
        var    dst   = new short[dstN];

        for (int i = 0; i < dstN; i++)
        {
            double pos = i * ratio;
            int    idx = (int)pos;
            double frc = pos - idx;
            float  a   = idx     < mono.Length ? mono[idx]     : 0f;
            float  b   = idx + 1 < mono.Length ? mono[idx + 1] : 0f;
            dst[i] = (short)Math.Clamp((a + frc * (b - a)) * 32767, -32768, 32767);
        }

        var outPath   = Path.ChangeExtension(inputPath, null) + "_16k.wav";
        int dataBytes = dstN * 2;

        using var output = File.Create(outPath);
        using var w      = new BinaryWriter(output);

        w.Write("RIFF"u8.ToArray()); w.Write(36 + dataBytes);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray()); w.Write(16);
        w.Write((short)1); w.Write((short)1);
        w.Write(WhisperSampleRate); w.Write(WhisperSampleRate * 2);
        w.Write((short)2); w.Write((short)16);
        w.Write("data"u8.ToArray()); w.Write(dataBytes);
        foreach (var s in dst) w.Write(s);

        return outPath;
    }

    private async Task<string> EnsureModelAsync(IProgress<string>? progress)
    {
        if (_modelPath is not null && File.Exists(_modelPath)) return _modelPath;

        var dest = Path.Combine(FileSystem.AppDataDirectory, "ggml-tiny.bin");
        if (!File.Exists(dest))
        {
            progress?.Report("Copying model to device...");
            using var src  = await FileSystem.OpenAppPackageFileAsync("ggml-tiny.bin");
            using var file = File.Create(dest);
            await src.CopyToAsync(file);
        }

        _modelPath = dest;
        return dest;
    }

    private static string GetLanguageCode() =>
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

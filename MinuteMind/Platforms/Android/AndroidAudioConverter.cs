using Android.Media;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class AndroidAudioConverter : IAudioConverter
{
    private const int TargetRate = 16000;

    public Task<string> ConvertToWavAsync(string inputPath, IProgress<string>? progress = null)
        => Task.Run(() => Convert(inputPath, progress));

    static string Convert(string inputPath, IProgress<string>? progress)
    {
        progress?.Report("Decoding audio...");

        using var extractor = new MediaExtractor();
        extractor.SetDataSource(inputPath);

        MediaFormat? audioFmt = null;
        int trackIdx = -1;

        for (int i = 0; i < extractor.TrackCount; i++)
        {
            var fmt = extractor.GetTrackFormat(i);
            var mime = fmt.GetString(MediaFormat.KeyMime) ?? "";
            if (mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                audioFmt = fmt;
                trackIdx = i;
                break;
            }
        }

        if (audioFmt == null)
            throw new InvalidOperationException(
                "No audio track found. Supported formats: WAV, M4A, MP3, AAC, OGG.");

        extractor.SelectTrack(trackIdx);

        var mimeType  = audioFmt.GetString(MediaFormat.KeyMime)!;
        int srcRate   = audioFmt.GetInteger(MediaFormat.KeySampleRate);
        int srcChans  = audioFmt.GetInteger(MediaFormat.KeyChannelCount);

        using var codec = MediaCodec.CreateDecoderByType(mimeType);
        codec.Configure(audioFmt, null, null, 0);
        codec.Start();

        var pcm  = new MemoryStream();
        var info = new MediaCodec.BufferInfo();
        bool endIn = false, endOut = false;

        while (!endOut)
        {
            if (!endIn)
            {
                int inIdx = codec.DequeueInputBuffer(10_000);
                if (inIdx >= 0)
                {
                    var inBuf = codec.GetInputBuffer(inIdx)!;
                    int n = extractor.ReadSampleData(inBuf, 0);
                    if (n < 0)
                    {
                        codec.QueueInputBuffer(inIdx, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
                        endIn = true;
                    }
                    else
                    {
                        codec.QueueInputBuffer(inIdx, 0, n, extractor.SampleTime, 0);
                        extractor.Advance();
                    }
                }
            }

            long outTimeout = endIn ? 100_000 : 10_000;
            int outIdx = codec.DequeueOutputBuffer(info, outTimeout);
            if (outIdx >= 0)
            {
                if ((info.Flags & MediaCodecBufferFlags.EndOfStream) != 0) endOut = true;
                if (info.Size > 0)
                {
                    var outBuf = codec.GetOutputBuffer(outIdx)!;
                    outBuf.Position(info.Offset);
                    var chunk = new byte[info.Size];
                    outBuf.Get(chunk);
                    pcm.Write(chunk);
                }
                codec.ReleaseOutputBuffer(outIdx, false);
            }
        }

        codec.Stop();

        var raw       = pcm.ToArray();
        int total     = raw.Length / 2 / srcChans; // 16-bit samples per channel

        // Mix to mono float
        var mono = new float[total];
        for (int i = 0; i < total; i++)
        {
            float s = 0f;
            for (int ch = 0; ch < srcChans; ch++)
            {
                int o = (i * srcChans + ch) * 2;
                if (o + 1 < raw.Length) s += BitConverter.ToInt16(raw, o) / 32768f;
            }
            mono[i] = s / srcChans;
        }

        // Resample to TargetRate
        short[] dst;
        if (srcRate == TargetRate)
        {
            dst = new short[total];
            for (int i = 0; i < total; i++) dst[i] = Clip(mono[i]);
        }
        else
        {
            progress?.Report("Resampling audio...");
            double ratio = (double)srcRate / TargetRate;
            int n = (int)(total / ratio);
            dst = new short[n];
            for (int i = 0; i < n; i++)
            {
                double pos = i * ratio;
                int    idx = (int)pos;
                double frc = pos - idx;
                float  a   = idx     < mono.Length ? mono[idx]     : 0f;
                float  b   = idx + 1 < mono.Length ? mono[idx + 1] : 0f;
                dst[i] = Clip(a + (float)(frc * (b - a)));
            }
        }

        // Write 16kHz mono 16-bit WAV
        var outPath   = Path.ChangeExtension(inputPath, null) + "_c.wav";
        int dataBytes = dst.Length * 2;

        using var file = File.Create(outPath);
        using var w    = new BinaryWriter(file);

        w.Write("RIFF"u8.ToArray()); w.Write(36 + dataBytes);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray()); w.Write(16);
        w.Write((short)1); w.Write((short)1); // PCM, mono
        w.Write(TargetRate); w.Write(TargetRate * 2); w.Write((short)2); w.Write((short)16);
        w.Write("data"u8.ToArray()); w.Write(dataBytes);
        foreach (var s in dst) w.Write(s);

        return outPath;
    }

    static short Clip(float v) => (short)Math.Clamp(v * 32767f, -32768f, 32767f);
}

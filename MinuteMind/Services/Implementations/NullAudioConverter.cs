using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class NullAudioConverter : IAudioConverter
{
    public Task<string> ConvertToWavAsync(string inputPath, IProgress<string>? progress = null) =>
        throw new NotSupportedException(
            "Audio conversion is not supported on this platform. Please upload a WAV file.");
}

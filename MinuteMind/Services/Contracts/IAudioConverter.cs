namespace MinuteMind.Services.Contracts;

public interface IAudioConverter
{
    Task<string> ConvertToWavAsync(string inputPath, IProgress<string>? progress = null);
}

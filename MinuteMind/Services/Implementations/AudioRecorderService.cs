using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class AudioRecorderService : IAudioRecorderService
{
    public bool IsRecording { get; private set; }
    public bool IsPaused { get; private set; }

    public Task<string> StartAsync()
    {
        IsRecording = true;
        IsPaused = false;
        return Task.FromResult(string.Empty);
    }

    public Task PauseAsync()
    {
        IsPaused = true;
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        IsPaused = false;
        return Task.CompletedTask;
    }

    public Task<string> StopAsync()
    {
        IsRecording = false;
        IsPaused = false;
        var path = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
        return Task.FromResult(path);
    }
}

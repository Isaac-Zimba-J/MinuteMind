namespace MinuteMind.Services.Contracts;

public interface IAudioRecorderService
{
    Task<string> StartAsync();
    Task PauseAsync();
    Task ResumeAsync();
    Task<string> StopAsync();
    bool IsRecording { get; }
    bool IsPaused { get; }
}

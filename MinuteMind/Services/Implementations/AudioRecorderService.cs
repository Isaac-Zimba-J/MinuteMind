using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class AudioRecorderService : IAudioRecorderService
{

    private readonly AudioManager _audioManager;
    private IAudioRecorder? _recorder;
    private string _filePath = string.Empty;

    public bool IsRecording { get; private set; }
    public bool IsPaused { get; private set; }

    public AudioRecorderService(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task<string> StartAsync()
    {
       // check/request microphone permission
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
                throw new Exception("Microphone permission is required to record audio.");
        }

        // Generate unique file path for recording
        _filePath = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

        // Create and start the audio recorder
        _recorder = _audioManager.CreateRecorder();
        await _recorder.StartAsync(_filePath);
        IsRecording = true;
        IsPaused = false;
        return _filePath;
    }

     public async Task PauseAsync()
    {
        if (_recorder is null) return;
        await _recorder.PauseAsync();
        IsPaused = true;
    }

    public async Task ResumeAsync()
    {
        if (_recorder is null) return;
        await _recorder.ResumeAsync();
        IsPaused = false;
    }

    public async Task<string> StopAsync()
    {
        if (_recorder is null) return string.Empty;
        await _recorder.StopAsync();
        IsRecording = false;
        IsPaused = false;
        return _filePath;
    }
}

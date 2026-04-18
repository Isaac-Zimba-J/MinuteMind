using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class RecordingViewModel(
    IAudioRecorderService audioRecorder,
    INavigationService navigationService) : ObservableObject
{
    [ObservableProperty]
    string meetingTitle = "Strategy Alignment Meeting";

    [ObservableProperty]
    string elapsedTime = "00:00:00";

    [ObservableProperty]
    string startedAt = $"Started at {DateTime.Now:h:mm tt}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    bool isPaused;

    [ObservableProperty]
    float[] waveformLevels = new float[13];

    private IDispatcherTimer? _waveformTimer;
    private readonly Random _rng = new();

    public string PauseButtonText => IsPaused ? "Resume" : "Pause";

    [ObservableProperty]
    string liveInsight = "Listening for speech...";

    private IDispatcherTimer? _timer;
    private TimeSpan _elapsed;

    [RelayCommand]
    async Task StartRecording()
    {
        await audioRecorder.StartAsync();
        _elapsed = TimeSpan.Zero;
        StartedAt = $"Started at {DateTime.Now:h:mm tt}";

        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer is not null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));
                ElapsedTime = _elapsed.ToString(@"hh\:mm\:ss");
            };
            _timer.Start();
        }

        _waveformTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_waveformTimer is not null)
        {
            _waveformTimer.Interval = TimeSpan.FromMilliseconds(150);
            _waveformTimer.Tick += (s, e) =>
            {
                var levels = new float[13];
                for (int i = 0; i < levels.Length; i++)
                    levels[i] = (float)(_rng.NextDouble() * 0.8 + 0.1);
                WaveformLevels = levels;
            };
            _waveformTimer.Start();
        }
    }

    [RelayCommand]
    async Task TogglePause()
    {
        if (IsPaused)
        {
            await audioRecorder.ResumeAsync();
            _timer?.Start();
            _waveformTimer?.Start();
            IsPaused = false;
        }
        else
        {
            await audioRecorder.PauseAsync();
            _timer?.Stop();
            _waveformTimer?.Stop();
            IsPaused = true;
        }
    }

    [RelayCommand]
    async Task StopRecording()
    {
        _timer?.Stop();
        _waveformTimer?.Stop();
        var audioPath = await audioRecorder.StopAsync();

        await navigationService.GoToAsync(nameof(Views.ProcessingPage),
            new Dictionary<string, object>
            {
                { "AudioPath", audioPath ?? string.Empty },
                { "MeetingTitle", MeetingTitle },
                { "Duration", _elapsed.Ticks }
            });
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class ProcessingViewModel(
    ITranscriptionService transcriptionService,
    IMinutesGeneratorService minutesGenerator,
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject, IQueryAttributable
{
    private string _audioPath = string.Empty;
    private string _meetingTitle = string.Empty;
    private long _durationTicks;

    [ObservableProperty]
    ObservableCollection<ProcessingStep> steps = new()
    {
        new ProcessingStep { Title = "Uploading audio...", Subtitle = "Preparing file", Status = StepStatus.Pending },
        new ProcessingStep { Title = "Transcribing audio...", Subtitle = "Waiting to start", Status = StepStatus.Pending },
        new ProcessingStep { Title = "Generating meeting minutes...", Subtitle = "Extracting action items next", Status = StepStatus.Pending },
    };

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("AudioPath", out var path))
            _audioPath = path?.ToString() ?? string.Empty;
        if (query.TryGetValue("MeetingTitle", out var title))
            _meetingTitle = title?.ToString() ?? "Untitled Meeting";
        if (query.TryGetValue("Duration", out var dur) && dur is long ticks)
            _durationTicks = ticks;

        StartProcessingCommand.Execute(null);
    }

    [RelayCommand]
    async Task StartProcessing()
    {
        try
        {
            await RunProcessingAsync();
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert(
                    "Processing Error",
                    $"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace?[..Math.Min(500, ex.StackTrace?.Length ?? 0)]}",
                    "OK");
            });
        }
    }

    async Task RunProcessingAsync()
    {
        // Step 1: Upload / prepare
        Steps[0].Status = StepStatus.Active;
        Steps[0].Subtitle = "Preparing file...";
        await Task.Delay(1200);
        Steps[0].Status = StepStatus.Completed;
        Steps[0].Subtitle = "Complete";

        // Step 2: Transcribe
        Steps[1].Status = StepStatus.Active;
        Steps[1].Subtitle = "Decoding speakers...";
        var segments = await transcriptionService.TranscribeAsync(
            _audioPath,
            new Progress<string>(msg => Steps[1].Subtitle = msg));
        Steps[1].Status = StepStatus.Completed;
        Steps[1].Subtitle = $"{segments.Count} segments found";

        // Step 3: Generate minutes
        Steps[2].Status = StepStatus.Active;
        Steps[2].Subtitle = "Analyzing transcript...";
        var minutes = await minutesGenerator.GenerateAsync(
            segments,
            new Progress<string>(msg =>
            {
                MainThread.BeginInvokeOnMainThread(() => Steps[2].Subtitle = msg);
            }));
        Steps[2].Status = StepStatus.Completed;
        Steps[2].Subtitle = "Minutes ready";

        // Save meeting
        var meeting = new Meeting
        {
            Title = _meetingTitle,
            DurationTicks = _durationTicks,
            Status = MeetingStatus.MinutesReady,
            TranscriptJson = System.Text.Json.JsonSerializer.Serialize(segments),
            MinutesJson = System.Text.Json.JsonSerializer.Serialize(minutes),
        };
        await meetingRepository.SaveAsync(meeting);

        await Task.Delay(600);

        // Navigate to minutes
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meeting.Id } });
    }
}

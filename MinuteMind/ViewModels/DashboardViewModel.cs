using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class DashboardViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoRecentMeetings))]
    ObservableCollection<Meeting> recentMeetings = new();

    public bool HasNoRecentMeetings => RecentMeetings.Count == 0;

    [RelayCommand]
    async Task LoadRecentMeetings()
    {
        var meetings = await meetingRepository.GetRecentAsync(3);
        RecentMeetings = new ObservableCollection<Meeting>(meetings);
    }

    [RelayCommand]
    async Task RecordMeeting()
    {
        await Shell.Current.GoToAsync("//RecordingPage");
    }

    [RelayCommand]
    async Task UploadAudio()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select an audio file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp4", "audio/x-m4a" } },
                { DevicePlatform.iOS, new[] { "public.mp3", "public.wav", "com.apple.m4a-audio" } },
            })
        });

        if (result is not null)
        {
            await navigationService.GoToAsync(nameof(Views.ProcessingPage),
                new Dictionary<string, object>
                {
                    { "AudioPath", result.FullPath },
                    { "MeetingTitle", Path.GetFileNameWithoutExtension(result.FileName) },
                    { "Duration", 0L }
                });
        }
    }

    [RelayCommand]
    async Task OpenMeeting(int meetingId)
    {
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meetingId } });
    }

    [RelayCommand]
    async Task ViewAllMeetings()
    {
        await Shell.Current.GoToAsync("//MeetingsPage");
    }
}

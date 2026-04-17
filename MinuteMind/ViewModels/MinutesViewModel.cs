using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    string meetingTitle = string.Empty;

    [ObservableProperty]
    string meetingDate = string.Empty;

    [ObservableProperty]
    string meetingDuration = string.Empty;

    [ObservableProperty]
    ObservableCollection<string> attendees = new();

    [ObservableProperty]
    ObservableCollection<string> discussionPoints = new();

    [ObservableProperty]
    ObservableCollection<Decision> decisions = new();

    [ObservableProperty]
    ObservableCollection<ActionItem> actionItems = new();

    private int _meetingId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("MeetingId", out var id))
        {
            _meetingId = Convert.ToInt32(id);
            LoadMeetingCommand.Execute(null);
        }
    }

    [RelayCommand]
    async Task LoadMeeting()
    {
        var meeting = await meetingRepository.GetByIdAsync(_meetingId);
        if (meeting is null) return;

        MeetingTitle = meeting.Title;
        MeetingDate = meeting.CreatedAt.ToString("MMM dd, yyyy");
        MeetingDuration = $"{meeting.Duration.TotalMinutes:0} mins";

        if (!string.IsNullOrEmpty(meeting.MinutesJson))
        {
            var minutes = JsonSerializer.Deserialize<MeetingMinutes>(meeting.MinutesJson);
            if (minutes is not null)
            {
                Attendees = new ObservableCollection<string>(minutes.Attendees);
                DiscussionPoints = new ObservableCollection<string>(minutes.DiscussionPoints);
                Decisions = new ObservableCollection<Decision>(minutes.Decisions);
                ActionItems = new ObservableCollection<ActionItem>(minutes.ActionItems);
            }
        }
    }

    [RelayCommand]
    async Task EditMinutes()
    {
        await navigationService.GoToAsync(nameof(Views.EditMinutesPage),
            new Dictionary<string, object> { { "MeetingId", _meetingId } });
    }

    [RelayCommand]
    async Task ExportPdf()
    {
        await navigationService.GoToAsync(nameof(Views.ExportPage),
            new Dictionary<string, object> { { "MeetingId", _meetingId } });
    }

    [RelayCommand]
    async Task ViewTranscript()
    {
        await navigationService.GoToAsync(nameof(Views.TranscriptPage),
            new Dictionary<string, object> { { "MeetingId", _meetingId } });
    }

    [RelayCommand]
    async Task GoBack()
    {
        await navigationService.GoBackAsync();
    }
}

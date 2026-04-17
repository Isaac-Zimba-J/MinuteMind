using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class EditMinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    string meetingTitle = string.Empty;

    [ObservableProperty]
    string meetingMeta = string.Empty;

    [ObservableProperty]
    string minutesText = string.Empty;

    private int _meetingId;
    private Meeting? _meeting;

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
        _meeting = await meetingRepository.GetByIdAsync(_meetingId);
        if (_meeting is null) return;

        MeetingTitle = _meeting.Title;
        MeetingMeta = $"{_meeting.CreatedAt:MMM dd, yyyy} • {_meeting.CreatedAt:HH:mm}";

        if (!string.IsNullOrEmpty(_meeting.MinutesJson))
        {
            var minutes = JsonSerializer.Deserialize<MeetingMinutes>(_meeting.MinutesJson);
            if (minutes is not null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Key Decisions:");
                foreach (var d in minutes.Decisions)
                    sb.AppendLine($"• [{d.Category}] {d.Text}");
                sb.AppendLine();
                sb.AppendLine("Action Items:");
                foreach (var a in minutes.ActionItems)
                    sb.AppendLine($"• {a.Description} — {a.Assignee} (due {a.DueDate:MMM dd})");
                sb.AppendLine();
                sb.AppendLine("Discussion Points:");
                foreach (var p in minutes.DiscussionPoints)
                    sb.AppendLine($"• {p}");

                MinutesText = sb.ToString();
            }
        }
    }

    [RelayCommand]
    async Task SaveChanges()
    {
        if (_meeting is null) return;

        _meeting.Title = MeetingTitle;
        await meetingRepository.SaveAsync(_meeting);
        await navigationService.GoBackAsync();
    }

    [RelayCommand]
    async Task Cancel()
    {
        await navigationService.GoBackAsync();
    }
}

using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class TranscriptViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    string meetingTitle = string.Empty;

    [ObservableProperty]
    string meetingMeta = string.Empty;

    [ObservableProperty]
    ObservableCollection<TranscriptSegment> segments = new();

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
        MeetingMeta = $"{meeting.CreatedAt:MMM dd, yyyy} • {meeting.Duration:mm\\:ss} duration";

        if (!string.IsNullOrEmpty(meeting.TranscriptJson))
        {
            var list = JsonSerializer.Deserialize<List<TranscriptSegment>>(meeting.TranscriptJson);
            if (list is not null)
            {
                Segments = new ObservableCollection<TranscriptSegment>(list);
            }
        }
    }

    [RelayCommand]
    async Task GenerateMinutes()
    {
        await navigationService.GoToAsync(nameof(Views.ProcessingPage),
            new Dictionary<string, object> { { "MeetingId", _meetingId } });
    }

    [RelayCommand]
    async Task GoBack()
    {
        await navigationService.GoBackAsync();
    }
}

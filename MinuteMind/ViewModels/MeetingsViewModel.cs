using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MeetingsViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<Meeting> meetings = new();

    [ObservableProperty]
    string searchQuery = string.Empty;

    [ObservableProperty]
    bool isLoading;

    public bool HasNoMeetings => Meetings.Count == 0;

    [RelayCommand]
    async Task LoadMeetings()
    {
        IsLoading = true;
        var all = await meetingRepository.GetAllAsync();
        Meetings = new ObservableCollection<Meeting>(all);
        IsLoading = false;
        OnPropertyChanged(nameof(HasNoMeetings));
    }

    [RelayCommand]
    async Task Search()
    {
        IsLoading = true;
        var all = await meetingRepository.GetAllAsync();
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            Meetings = new ObservableCollection<Meeting>(all);
        }
        else
        {
            var filtered = all.Where(m =>
                m.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            Meetings = new ObservableCollection<Meeting>(filtered);
        }
        IsLoading = false;
        OnPropertyChanged(nameof(HasNoMeetings));
    }

    [RelayCommand]
    async Task OpenMeeting(int meetingId)
    {
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meetingId } });
    }

    [RelayCommand]
    async Task DeleteMeeting(Meeting meeting)
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Delete Meeting",
            $"Delete \"{meeting.Title}\"? This cannot be undone.",
            "Delete", "Cancel");

        if (!confirmed) return;

        await meetingRepository.DeleteAsync(meeting);
        Meetings.Remove(meeting);
        OnPropertyChanged(nameof(HasNoMeetings));
    }
}

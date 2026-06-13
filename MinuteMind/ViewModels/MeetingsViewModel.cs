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
        Meetings.Clear();
        foreach (var m in all) Meetings.Add(m);
        IsLoading = false;
        OnPropertyChanged(nameof(HasNoMeetings));
    }

    [RelayCommand]
    async Task Search()
    {
        IsLoading = true;
        var all = await meetingRepository.GetAllAsync();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? all
            : all.Where(m => m.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        Meetings.Clear();
        foreach (var m in filtered) Meetings.Add(m);
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

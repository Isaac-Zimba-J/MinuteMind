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

    [RelayCommand]
    async Task LoadMeetings()
    {
        IsLoading = true;
        var all = await meetingRepository.GetAllAsync();
        Meetings = new ObservableCollection<Meeting>(all);
        IsLoading = false;
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
    }

    [RelayCommand]
    async Task OpenMeeting(int meetingId)
    {
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meetingId } });
    }
}

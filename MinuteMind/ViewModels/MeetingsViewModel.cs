using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MeetingsViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}

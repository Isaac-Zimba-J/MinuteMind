using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}

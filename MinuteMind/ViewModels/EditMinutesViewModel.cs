using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class EditMinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}

using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class TranscriptViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}

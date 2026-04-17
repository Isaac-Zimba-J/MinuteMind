using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class RecordingViewModel(
    IAudioRecorderService audioRecorder,
    INavigationService navigationService) : ObservableObject
{
}

using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class ProcessingViewModel(
    ITranscriptionService transcriptionService,
    IMinutesGeneratorService minutesGenerator,
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}

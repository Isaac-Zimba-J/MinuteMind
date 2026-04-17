using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class ExportViewModel(
    IMeetingRepository meetingRepository,
    IPdfExportService pdfExportService,
    INavigationService navigationService) : ObservableObject
{
}

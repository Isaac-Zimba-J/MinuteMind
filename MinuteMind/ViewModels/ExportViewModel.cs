using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class ExportViewModel(
    IMeetingRepository meetingRepository,
    IPdfExportService pdfExportService,
    INavigationService navigationService) : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    string meetingTitle = string.Empty;

    [ObservableProperty]
    string exportDate = string.Empty;

    [ObservableProperty]
    bool isExporting;

    [ObservableProperty]
    bool exportComplete;

    [ObservableProperty]
    string exportedFilePath = string.Empty;

    private int _meetingId;
    private Meeting? _meeting;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("MeetingId", out var id))
        {
            _meetingId = Convert.ToInt32(id);
            LoadMeetingCommand.Execute(null);
        }
    }

    [RelayCommand]
    async Task LoadMeeting()
    {
        _meeting = await meetingRepository.GetByIdAsync(_meetingId);
        if (_meeting is null) return;

        MeetingTitle = _meeting.Title;
        ExportDate = $"Exported on {DateTime.Now:MMM dd, yyyy}";
    }

    [RelayCommand]
    async Task DownloadPdf()
    {
        if (_meeting is null) return;

        IsExporting = true;
        ExportedFilePath = await pdfExportService.ExportAsync(_meeting);
        IsExporting = false;
        ExportComplete = true;
    }

    [RelayCommand]
    async Task SharePdf()
    {
        if (string.IsNullOrEmpty(ExportedFilePath)) return;

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = MeetingTitle,
            File = new ShareFile(ExportedFilePath)
        });
    }

    [RelayCommand]
    async Task Close()
    {
        await navigationService.GoBackAsync();
    }
}

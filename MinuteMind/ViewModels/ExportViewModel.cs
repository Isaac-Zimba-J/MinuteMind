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
    Task DownloadPdf() => ExportAndShareAsync();

    [RelayCommand]
    Task SharePdf() => ExportAndShareAsync();

    async Task ExportAndShareAsync()
    {
        if (_meeting is null) return;

        try
        {
            if (string.IsNullOrEmpty(ExportedFilePath) || !File.Exists(ExportedFilePath))
            {
                IsExporting = true;
                ExportedFilePath = await pdfExportService.ExportAsync(_meeting);
                IsExporting = false;
                ExportComplete = true;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = MeetingTitle,
                File = new ShareFile(ExportedFilePath)
            });
        }
        catch (Exception ex)
        {
            IsExporting = false;
            var inner = ex.InnerException;
            var msg = ex.Message;
            while (inner is not null)
            {
                msg += $"\n→ {inner.Message}";
                inner = inner.InnerException;
            }
            await Shell.Current.DisplayAlert("Export Failed", msg, "OK");
        }
    }

    [RelayCommand]
    async Task Close()
    {
        await navigationService.GoBackAsync();
    }
}

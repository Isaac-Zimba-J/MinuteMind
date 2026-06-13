using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IMeetingRepository _meetingRepository;

    private static readonly string[] Languages =
    [
        "English", "Spanish", "French", "German", "Portuguese",
        "Italian", "Chinese", "Japanese", "Korean", "Arabic", "Hindi"
    ];

    public SettingsViewModel(IMeetingRepository meetingRepository)
    {
        _meetingRepository = meetingRepository;
        isDarkMode      = Preferences.Default.Get("settings_dark_mode", false);
        autoTranscribe  = Preferences.Default.Get("settings_auto_transcribe", true);
        selectedLanguage = Preferences.Default.Get("settings_language", "English");
        groqApiKey      = Preferences.Default.Get("groq_api_key", string.Empty);
        ApplyTheme(isDarkMode);
    }

    [ObservableProperty]
    bool isDarkMode;

    [ObservableProperty]
    bool autoTranscribe;

    [ObservableProperty]
    string selectedLanguage;

    [ObservableProperty]
    string groqApiKey;

    [ObservableProperty]
    string appVersion = AppInfo.VersionString;

    partial void OnGroqApiKeyChanged(string value)      => Preferences.Default.Set("groq_api_key", value);
    partial void OnAutoTranscribeChanged(bool value)    => Preferences.Default.Set("settings_auto_transcribe", value);
    partial void OnSelectedLanguageChanged(string value) => Preferences.Default.Set("settings_language", value);
    partial void OnIsDarkModeChanged(bool value)
    {
        Preferences.Default.Set("settings_dark_mode", value);
        ApplyTheme(value);
    }

    [RelayCommand]
    async Task PickLanguage()
    {
        var picked = await Shell.Current.DisplayActionSheetAsync(
            "Transcription Language", "Cancel", null, Languages);
        if (!string.IsNullOrEmpty(picked) && picked != "Cancel")
            SelectedLanguage = picked;
    }

    [RelayCommand]
    async Task DeleteAllMeetings()
    {
        bool confirmed = await Shell.Current.DisplayAlertAsync(
            "Delete All Meetings",
            "This will permanently delete all meetings, transcripts, and minutes. This cannot be undone.",
            "Delete All", "Cancel");
        if (!confirmed) return;
        await _meetingRepository.DeleteAllAsync();
    }

    private static void ApplyTheme(bool isDark) =>
        Application.Current!.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}

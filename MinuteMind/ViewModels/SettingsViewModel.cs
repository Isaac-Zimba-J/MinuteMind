using CommunityToolkit.Mvvm.ComponentModel;

namespace MinuteMind.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
        isDarkMode = Preferences.Default.Get("settings_dark_mode", false);
        autoTranscribe = Preferences.Default.Get("settings_auto_transcribe", true);
        selectedLanguage = Preferences.Default.Get("settings_language", "English");
        ApplyTheme(isDarkMode);
    }

    [ObservableProperty]
    bool isDarkMode;

    [ObservableProperty]
    bool autoTranscribe;

    [ObservableProperty]
    string selectedLanguage;

    [ObservableProperty]
    string appVersion = AppInfo.VersionString;

    partial void OnIsDarkModeChanged(bool value)
    {
        Preferences.Default.Set("settings_dark_mode", value);
        ApplyTheme(value);
    }

    partial void OnAutoTranscribeChanged(bool value)
    {
        Preferences.Default.Set("settings_auto_transcribe", value);
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        Preferences.Default.Set("settings_language", value);
    }

    private static void ApplyTheme(bool isDark) =>
        Application.Current!.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}

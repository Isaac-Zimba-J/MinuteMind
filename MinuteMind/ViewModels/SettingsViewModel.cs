using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MinuteMind.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    bool isDarkMode;

    [ObservableProperty]
    bool autoTranscribe = true;

    [ObservableProperty]
    string selectedLanguage = "English";

    [ObservableProperty]
    string appVersion = "1.0.0";

    [RelayCommand]
    void ToggleDarkMode()
    {
        // Will be wired to theming in Phase 4
    }
}

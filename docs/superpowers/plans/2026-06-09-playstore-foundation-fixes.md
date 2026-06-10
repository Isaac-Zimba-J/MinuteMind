# MinuteMind — Play Store Foundation Fixes — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all hardcoded/stub data and missing UX with real functionality — fix the build, wire Dashboard to SQLite, persist Settings, add meeting deletion, wire empty states, handle export errors, and show actionable permission denied messages.

**Architecture:** All changes follow the existing MVVM pattern. ViewModels get new `[ObservableProperty]` fields and `[RelayCommand]` methods. XAML binds via `EventToCommandBehavior` (CommunityToolkit.Maui). No new services or interfaces needed — all changes are additive.

**Tech Stack:** CommunityToolkit.Mvvm 8.4.2, CommunityToolkit.Maui 14.1.0, `Preferences.Default`, `AppInfo.ShowSettingsUI()`

**Scope:** This plan covers PLAYSTORE_READINESS.md items 1, 7, 8, 10, 13, 14, 25, 26. A second plan covers AI minutes, Android foreground service, MediaStore PDF, and app signing.

---

## File Map

| File | Action |
|------|--------|
| `MinuteMind/ViewModels/DashboardViewModel.cs` | Modify — add `RecentMeetings` + `LoadRecentMeetingsCommand` |
| `MinuteMind/ViewModels/SettingsViewModel.cs` | Modify — persist + apply all settings via `Preferences.Default` |
| `MinuteMind/ViewModels/MeetingsViewModel.cs` | Modify — add `DeleteMeetingCommand` + `HasNoMeetings` + notify on load |
| `MinuteMind/ViewModels/RecordingViewModel.cs` | Modify — wrap `StartRecording` in try/catch for permission denied |
| `MinuteMind/ViewModels/ExportViewModel.cs` | Modify — wrap `DownloadPdf` in try/catch with `ExportError` property |
| `MinuteMind/Views/Dashboard.xaml` | Modify — replace 3 hardcoded cards with `BindableLayout` + empty state |
| `MinuteMind/Views/MeetingsPage.xaml` | Modify — add `SwipeView` delete, wire empty state, add `EventToCommandBehavior` |
| `MinuteMind/Views/ExportPage.xaml` | Modify — add error label bound to `ExportError` |
| `MinuteMind/Converters/StringNotEmptyConverter.cs` | Create — `IValueConverter` for string-not-empty binding |
| `MinuteMind/Resources/Styles/Styles.xaml` | Modify — register `StringNotEmptyConverter` |

---

## Task 1: Fix the Android Build

**Files:** Depends on compiler output.

- [ ] **Step 1: Run the build and capture the first error**

```bash
cd "/Users/zimbadev/Documents/Workspace/Maui Projects/MinuteMind"
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android 2>&1 | grep " error " | head -20
```

Expected: one or more lines like `MinuteMind/SomeFile.cs(42,10): error CS0246: The type or namespace name 'Foo' could not be found`

- [ ] **Step 2: Fix the first compiler error shown**

Open the file at the line reported. Read the error message — it will say exactly what is wrong (missing using, wrong type name, API change, etc.). Fix that line.

- [ ] **Step 3: Re-run until the build succeeds**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android 2>&1 | tail -5
```

Expected output ending with: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add -u
git commit -m "fix: resolve Android build compilation errors"
```

---

## Task 2: Wire Dashboard to Real Meeting Data

**Files:**
- Modify: `MinuteMind/ViewModels/DashboardViewModel.cs`
- Modify: `MinuteMind/Views/Dashboard.xaml`

- [ ] **Step 1: Replace DashboardViewModel.cs**

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class DashboardViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoRecentMeetings))]
    ObservableCollection<Meeting> recentMeetings = new();

    public bool HasNoRecentMeetings => RecentMeetings.Count == 0;

    [RelayCommand]
    async Task LoadRecentMeetings()
    {
        var meetings = await meetingRepository.GetRecentAsync(3);
        RecentMeetings = new ObservableCollection<Meeting>(meetings);
    }

    [RelayCommand]
    async Task RecordMeeting()
    {
        await Shell.Current.GoToAsync("//RecordingPage");
    }

    [RelayCommand]
    async Task UploadAudio()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select an audio file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp4", "audio/x-m4a" } },
                { DevicePlatform.iOS, new[] { "public.mp3", "public.wav", "com.apple.m4a-audio" } },
            })
        });

        if (result is not null)
        {
            await navigationService.GoToAsync(nameof(Views.ProcessingPage),
                new Dictionary<string, object>
                {
                    { "AudioPath", result.FullPath },
                    { "MeetingTitle", Path.GetFileNameWithoutExtension(result.FileName) },
                    { "Duration", 0L }
                });
        }
    }

    [RelayCommand]
    async Task OpenMeeting(int meetingId)
    {
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meetingId } });
    }

    [RelayCommand]
    async Task ViewAllMeetings()
    {
        await Shell.Current.GoToAsync("//MeetingsPage");
    }
}
```

- [ ] **Step 2: Add namespaces to Dashboard.xaml**

In the `<ContentPage ...>` opening tag, add these two namespace declarations alongside the existing ones:

```xml
xmlns:models="clr-namespace:MinuteMind.Models"
xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
```

- [ ] **Step 3: Add EventToCommandBehavior to trigger load on appear**

Immediately after the opening `<ContentPage ...>` tag (before `<Grid ...>`), insert:

```xml
<ContentPage.Behaviors>
    <toolkit:EventToCommandBehavior EventName="Appearing"
                                    Command="{Binding LoadRecentMeetingsCommand}" />
</ContentPage.Behaviors>
```

- [ ] **Step 4: Replace the 3 hardcoded meeting cards**

Find the `<!-- Meeting Cards -->` comment and the `<VerticalStackLayout Spacing="16">` block containing the three hardcoded `<Border>` elements. Replace the entire `<VerticalStackLayout Spacing="16">` block with:

```xml
<VerticalStackLayout Spacing="16"
                     BindableLayout.ItemsSource="{Binding RecentMeetings}">
    <BindableLayout.EmptyView>
        <VerticalStackLayout HorizontalOptions="Center"
                             Spacing="12"
                             Margin="0,24,0,0">
            <Label Text="No meetings recorded yet"
                   FontFamily="PlusJakartaSemiBold"
                   FontSize="16"
                   TextColor="{StaticResource OnSurfaceVariant}"
                   HorizontalTextAlignment="Center" />
            <Label Text="Tap Record Meeting to get started."
                   FontFamily="InterRegular"
                   FontSize="14"
                   TextColor="{StaticResource OnSurfaceVariant}"
                   HorizontalTextAlignment="Center" />
        </VerticalStackLayout>
    </BindableLayout.EmptyView>
    <BindableLayout.ItemTemplate>
        <DataTemplate x:DataType="models:Meeting">
            <Border BackgroundColor="{StaticResource SurfaceContainerLowest}"
                    Padding="20"
                    StrokeThickness="0"
                    StrokeShape="RoundRectangle 24">
                <Border.Shadow>
                    <Shadow Brush="{StaticResource OnSurfaceBrush}"
                            Offset="0,12" Radius="32" Opacity="0.04" />
                </Border.Shadow>
                <Border.GestureRecognizers>
                    <TapGestureRecognizer
                        Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.OpenMeetingCommand}"
                        CommandParameter="{Binding Id}" />
                </Border.GestureRecognizers>
                <Grid ColumnDefinitions="56,*,Auto" ColumnSpacing="16">
                    <Border WidthRequest="56" HeightRequest="56"
                            BackgroundColor="{StaticResource SurfaceContainer}"
                            StrokeThickness="0" StrokeShape="RoundRectangle 16">
                        <Image Source="doc.png" HeightRequest="22" WidthRequest="22"
                               HorizontalOptions="Center" VerticalOptions="Center" />
                    </Border>
                    <VerticalStackLayout Grid.Column="1" Spacing="6" VerticalOptions="Center">
                        <Label Text="{Binding Title}"
                               FontFamily="PlusJakartaSemiBold" FontSize="16"
                               TextColor="{StaticResource OnSurface}"
                               LineBreakMode="TailTruncation" />
                        <Label Text="{Binding CreatedAt, StringFormat='{0:MMM dd, yyyy}'}"
                               FontFamily="InterRegular" FontSize="12"
                               TextColor="{StaticResource OnSurfaceVariant}" />
                    </VerticalStackLayout>
                    <Image Grid.Column="2" Source="chevron.png"
                           HeightRequest="20" WidthRequest="20"
                           VerticalOptions="Center" />
                </Grid>
            </Border>
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</VerticalStackLayout>
```

- [ ] **Step 5: Verify**

Run the app. Navigate to the Dashboard tab.
- With no recordings: "No meetings recorded yet" message appears.
- With existing recordings: real titles and dates show, tapping opens MinutesPage.

- [ ] **Step 6: Commit**

```bash
git add MinuteMind/ViewModels/DashboardViewModel.cs MinuteMind/Views/Dashboard.xaml
git commit -m "feat: wire Dashboard recent meetings to SQLite via MeetingRepository"
```

---

## Task 3: Persist Settings

**Files:**
- Modify: `MinuteMind/ViewModels/SettingsViewModel.cs`

- [ ] **Step 1: Replace SettingsViewModel.cs**

```csharp
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
```

- [ ] **Step 2: Verify persistence**

Run app → Settings → toggle Dark Mode ON → kill app → relaunch.
Expected: app opens in dark mode and the toggle is still ON.

- [ ] **Step 3: Commit**

```bash
git add MinuteMind/ViewModels/SettingsViewModel.cs
git commit -m "feat: persist dark mode, auto-transcribe, and language settings via Preferences"
```

---

## Task 4: Meeting Deletion with Swipe-to-Delete

**Files:**
- Modify: `MinuteMind/ViewModels/MeetingsViewModel.cs`
- Modify: `MinuteMind/Views/MeetingsPage.xaml`

- [ ] **Step 1: Add DeleteMeeting command and HasNoMeetings to MeetingsViewModel.cs**

After the `OpenMeeting` command, add:

```csharp
public bool HasNoMeetings => Meetings.Count == 0;

[RelayCommand]
async Task DeleteMeeting(Meeting meeting)
{
    var confirmed = await Shell.Current.DisplayAlert(
        "Delete Meeting",
        $"Delete \"{meeting.Title}\"? This cannot be undone.",
        "Delete", "Cancel");

    if (!confirmed) return;

    await meetingRepository.DeleteAsync(meeting);
    Meetings.Remove(meeting);
    OnPropertyChanged(nameof(HasNoMeetings));
}
```

- [ ] **Step 2: Notify HasNoMeetings after data loads**

In the `LoadMeetings` command, add `OnPropertyChanged(nameof(HasNoMeetings));` as the last line (after `IsLoading = false;`):

```csharp
[RelayCommand]
async Task LoadMeetings()
{
    IsLoading = true;
    var all = await meetingRepository.GetAllAsync();
    Meetings = new ObservableCollection<Meeting>(all);
    IsLoading = false;
    OnPropertyChanged(nameof(HasNoMeetings));
}
```

- [ ] **Step 3: Add toolkit namespace and EventToCommandBehavior to MeetingsPage.xaml**

Add `xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"` to the `<ContentPage ...>` tag.

Add this block immediately after the opening `<ContentPage ...>` tag:

```xml
<ContentPage.Behaviors>
    <toolkit:EventToCommandBehavior EventName="Appearing"
                                    Command="{Binding LoadMeetingsCommand}" />
</ContentPage.Behaviors>
```

- [ ] **Step 4: Restructure the ScrollView content in MeetingsPage.xaml**

Replace the entire `<ScrollView Grid.Row="1" ...>` block with:

```xml
<ScrollView Grid.Row="1" VerticalScrollBarVisibility="Never">
    <VerticalStackLayout Padding="24,24,24,24" Spacing="16">

        <!-- Meeting items with swipe-to-delete -->
        <VerticalStackLayout Spacing="16"
                             BindableLayout.ItemsSource="{Binding Meetings}">
            <BindableLayout.ItemTemplate>
                <DataTemplate x:DataType="models:Meeting">
                    <SwipeView>
                        <SwipeView.RightItems>
                            <SwipeItems Mode="Execute">
                                <SwipeItem Text="Delete"
                                           BackgroundColor="{StaticResource Error}"
                                           Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.DeleteMeetingCommand}"
                                           CommandParameter="{Binding .}" />
                            </SwipeItems>
                        </SwipeView.RightItems>
                        <Border BackgroundColor="{StaticResource SurfaceContainerLowest}"
                                Padding="20"
                                StrokeThickness="0"
                                StrokeShape="RoundRectangle 24">
                            <Border.Shadow>
                                <Shadow Brush="{StaticResource OnSurfaceBrush}"
                                        Offset="0,12" Radius="32" Opacity="0.04" />
                            </Border.Shadow>
                            <Border.GestureRecognizers>
                                <TapGestureRecognizer
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.OpenMeetingCommand}"
                                    CommandParameter="{Binding Id}" />
                            </Border.GestureRecognizers>
                            <Grid ColumnDefinitions="56,*,Auto" ColumnSpacing="16">
                                <Border WidthRequest="56" HeightRequest="56"
                                        BackgroundColor="{StaticResource SurfaceContainer}"
                                        StrokeThickness="0" StrokeShape="RoundRectangle 16">
                                    <Image Source="doc.png" HeightRequest="22" WidthRequest="22"
                                           HorizontalOptions="Center" VerticalOptions="Center" />
                                </Border>
                                <VerticalStackLayout Grid.Column="1" Spacing="6" VerticalOptions="Center">
                                    <Label Text="{Binding Title}"
                                           FontFamily="PlusJakartaSemiBold" FontSize="16"
                                           TextColor="{StaticResource OnSurface}"
                                           LineBreakMode="TailTruncation" />
                                    <HorizontalStackLayout Spacing="8">
                                        <Label Text="{Binding CreatedAt, StringFormat='{0:MMM dd, yyyy}'}"
                                               FontFamily="InterRegular" FontSize="12"
                                               TextColor="{StaticResource OnSurfaceVariant}" />
                                        <Border WidthRequest="4" HeightRequest="4"
                                                BackgroundColor="{StaticResource OutlineVariant}"
                                                StrokeThickness="0" StrokeShape="RoundRectangle 2"
                                                VerticalOptions="Center" />
                                        <Label Text="{Binding Duration, StringFormat='{0:mm\\:ss}'}"
                                               FontFamily="InterRegular" FontSize="12"
                                               TextColor="{StaticResource OnSurfaceVariant}" />
                                    </HorizontalStackLayout>
                                </VerticalStackLayout>
                                <Label Grid.Column="2" Text="&#x203A;"
                                       FontSize="24"
                                       TextColor="{StaticResource OnSurfaceVariant}"
                                       VerticalOptions="Center" />
                            </Grid>
                        </Border>
                    </SwipeView>
                </DataTemplate>
            </BindableLayout.ItemTemplate>
        </VerticalStackLayout>

        <!-- Empty state -->
        <VerticalStackLayout IsVisible="{Binding HasNoMeetings}"
                             HorizontalOptions="Center"
                             Spacing="12"
                             Margin="0,80,0,0">
            <Label Text="No meetings yet"
                   FontFamily="PlusJakartaSemiBold" FontSize="18"
                   TextColor="{StaticResource OnSurfaceVariant}"
                   HorizontalTextAlignment="Center" />
            <Label Text="Record or upload your first meeting to get started."
                   FontFamily="InterRegular" FontSize="14"
                   TextColor="{StaticResource OnSurfaceVariant}"
                   HorizontalTextAlignment="Center" />
        </VerticalStackLayout>

    </VerticalStackLayout>
</ScrollView>
```

- [ ] **Step 5: Verify**

Run app → Meetings tab. Swipe left on any meeting card → red "Delete" appears. Tap it → confirm dialog → meeting removed. Navigate away and back — it stays gone.

With all meetings deleted: empty state shows.

- [ ] **Step 6: Commit**

```bash
git add MinuteMind/ViewModels/MeetingsViewModel.cs MinuteMind/Views/MeetingsPage.xaml
git commit -m "feat: add swipe-to-delete for meetings, wire empty state and auto-load on appear"
```

---

## Task 5: Add Error Handling to PDF Export

**Files:**
- Create: `MinuteMind/Converters/StringNotEmptyConverter.cs`
- Modify: `MinuteMind/Resources/Styles/Styles.xaml`
- Modify: `MinuteMind/ViewModels/ExportViewModel.cs`
- Modify: `MinuteMind/Views/ExportPage.xaml`

- [ ] **Step 1: Create StringNotEmptyConverter.cs**

```csharp
using System.Globalization;

namespace MinuteMind.Converters;

public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

- [ ] **Step 2: Register converter in Styles.xaml**

Open `MinuteMind/Resources/Styles/Styles.xaml`. Add the converters namespace to the `<ResourceDictionary>` tag:

```xml
xmlns:converters="clr-namespace:MinuteMind.Converters"
```

Add the converter as a resource inside `<ResourceDictionary>`:

```xml
<converters:StringNotEmptyConverter x:Key="StringNotEmptyConverter" />
```

- [ ] **Step 3: Update ExportViewModel.cs — add ExportError and fix DownloadPdf**

Add the `ExportError` property after the existing `ExportedFilePath` property:

```csharp
[ObservableProperty]
string exportError = string.Empty;
```

Replace the `DownloadPdf` command with:

```csharp
[RelayCommand]
async Task DownloadPdf()
{
    if (_meeting is null) return;

    IsExporting = true;
    ExportError = string.Empty;
    try
    {
        ExportedFilePath = await pdfExportService.ExportAsync(_meeting);
        ExportComplete = true;
    }
    catch (Exception ex)
    {
        ExportError = $"Could not create PDF: {ex.Message}";
    }
    finally
    {
        IsExporting = false;
    }
}
```

- [ ] **Step 4: Add error label to ExportPage.xaml**

Open `MinuteMind/Views/ExportPage.xaml`. Locate the Download PDF button. Immediately after the button, add:

```xml
<Label Text="{Binding ExportError}"
       IsVisible="{Binding ExportError, Converter={StaticResource StringNotEmptyConverter}}"
       TextColor="{StaticResource Error}"
       FontFamily="InterRegular"
       FontSize="14"
       HorizontalTextAlignment="Center"
       Margin="0,8,0,0" />
```

- [ ] **Step 5: Verify**

Normal path: tap Download on a meeting with minutes — PDF generates, no error shows.

Error path: temporarily add `throw new Exception("disk full");` as the first line of `PdfExportService.ExportAsync`, run again — red error label "Could not create PDF: disk full" appears. Revert the temporary throw.

- [ ] **Step 6: Commit**

```bash
git add MinuteMind/Converters/StringNotEmptyConverter.cs \
        MinuteMind/Resources/Styles/Styles.xaml \
        MinuteMind/ViewModels/ExportViewModel.cs \
        MinuteMind/Views/ExportPage.xaml
git commit -m "feat: add error handling and error display to PDF export"
```

---

## Task 6: Permission Denied UX on Recording

**Files:**
- Modify: `MinuteMind/ViewModels/RecordingViewModel.cs`

- [ ] **Step 1: Wrap StartRecording in try/catch**

Replace the entire `StartRecording` command with:

```csharp
[RelayCommand]
async Task StartRecording()
{
    try
    {
        IsRecording = true;
        await audioRecorder.StartAsync();
        _elapsed = TimeSpan.Zero;
        StartedAt = $"Started at {DateTime.Now:h:mm tt}";

        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer is not null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));
                ElapsedTime = _elapsed.ToString(@"hh\:mm\:ss");
            };
            _timer.Start();
        }

        _waveformTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_waveformTimer is not null)
        {
            _waveformTimer.Interval = TimeSpan.FromMilliseconds(150);
            _waveformTimer.Tick += (s, e) =>
            {
                var levels = new float[13];
                for (int i = 0; i < levels.Length; i++)
                    levels[i] = (float)(_rng.NextDouble() * 0.8 + 0.1);
                WaveformLevels = levels;
            };
            _waveformTimer.Start();
        }
    }
    catch (Exception)
    {
        IsRecording = false;
        var openSettings = await Shell.Current.DisplayAlert(
            "Microphone Access Required",
            "MinuteMind needs microphone access to record meetings. Please allow it in your device settings.",
            "Open Settings",
            "Cancel");

        if (openSettings)
            AppInfo.ShowSettingsUI();
    }
}
```

- [ ] **Step 2: Verify**

On a real Android device: Settings → Apps → MinuteMind → Permissions → revoke Microphone. Open app → tap Record.
Expected: alert appears with "Open Settings" and "Cancel" buttons. Tapping "Open Settings" takes you to the app's permission settings.

- [ ] **Step 3: Commit**

```bash
git add MinuteMind/ViewModels/RecordingViewModel.cs
git commit -m "feat: show permission denied alert with Open Settings deep-link on recording"
```

---

## What This Plan Does NOT Cover

The following items require a **second plan** (more complex, platform-specific, or new dependencies):

| Item | Why Separate |
|------|-------------|
| AI minutes generation (Claude API) | New NuGet dependency, network calls, API key management, Settings UI change |
| Android foreground service | Platform-specific Android service class, notification channel setup |
| MediaStore PDF to Downloads | Platform-specific Android API, `DependencyService` abstraction needed |
| App signing + ProGuard | Build configuration, keystore management, R8 rule authoring |
| Real audio waveform levels | Requires `Plugin.Maui.Audio` amplitude API investigation |

After completing all 6 tasks above, create and execute the second plan for those items.

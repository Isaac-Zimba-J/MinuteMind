# MinuteMind Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a polished .NET MAUI meeting assistant app with real audio recording, local on-device transcription, SQLite storage, and PDF export across 9 screens.

**Architecture:** MVVM with CommunityToolkit.Mvvm. Shell-based 4-tab navigation. SQLite for persistence with JSON-serialized nested data. Service layer behind interfaces for testability and future AI swap-in.

**Tech Stack:** .NET 10 MAUI, CommunityToolkit.Mvvm 8.4.2, CommunityToolkit.Maui 14.1.0, sqlite-net-pcl, Syncfusion.Maui.Toolkit, Plugin.Maui.Audio, Whisper.net

**Design Reference:** Mockups at `MinuteMind/mockups/` and spec at `docs/superpowers/specs/2026-04-17-minutemind-design.md`

---

## Phase 1: Foundation (Theme, Fonts, Navigation)

### Task 1: Rewrite Colors.xaml with M3 Tonal Palette

**Files:**
- Modify: `MinuteMind/Resources/Styles/Colors.xaml`

- [ ] **Step 1: Replace Colors.xaml with the full M3 tonal palette**

Replace the entire content of `MinuteMind/Resources/Styles/Colors.xaml` with:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<?xaml-comp compile="true" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- M3 Tonal Color Palette — sourced from Stitch mockup Tailwind config -->

    <!-- Primary -->
    <Color x:Key="Primary">#005FAA</Color>
    <Color x:Key="PrimaryContainer">#0078D4</Color>
    <Color x:Key="OnPrimary">#FFFFFF</Color>
    <Color x:Key="OnPrimaryContainer">#FFFFFF</Color>
    <Color x:Key="PrimaryFixed">#D3E3FF</Color>
    <Color x:Key="PrimaryFixedDim">#A3C9FF</Color>
    <Color x:Key="OnPrimaryFixed">#001C39</Color>
    <Color x:Key="OnPrimaryFixedVariant">#004883</Color>

    <!-- Secondary -->
    <Color x:Key="Secondary">#456084</Color>
    <Color x:Key="SecondaryContainer">#B8D3FD</Color>
    <Color x:Key="OnSecondary">#FFFFFF</Color>
    <Color x:Key="OnSecondaryContainer">#405B7F</Color>
    <Color x:Key="SecondaryFixed">#D3E3FF</Color>
    <Color x:Key="SecondaryFixedDim">#ADC8F2</Color>
    <Color x:Key="OnSecondaryFixed">#001C39</Color>
    <Color x:Key="OnSecondaryFixedVariant">#2D486B</Color>

    <!-- Tertiary -->
    <Color x:Key="Tertiary">#974700</Color>
    <Color x:Key="TertiaryContainer">#BC5B00</Color>
    <Color x:Key="OnTertiary">#FFFFFF</Color>
    <Color x:Key="OnTertiaryContainer">#FFFFFF</Color>
    <Color x:Key="TertiaryFixed">#FFDBC8</Color>
    <Color x:Key="TertiaryFixedDim">#FFB689</Color>
    <Color x:Key="OnTertiaryFixed">#311300</Color>
    <Color x:Key="OnTertiaryFixedVariant">#743500</Color>

    <!-- Error -->
    <Color x:Key="Error">#BA1A1A</Color>
    <Color x:Key="ErrorContainer">#FFDAD6</Color>
    <Color x:Key="OnError">#FFFFFF</Color>
    <Color x:Key="OnErrorContainer">#93000A</Color>

    <!-- Surface -->
    <Color x:Key="Surface">#FAF9F8</Color>
    <Color x:Key="SurfaceBright">#FAF9F8</Color>
    <Color x:Key="SurfaceDim">#DADAD9</Color>
    <Color x:Key="SurfaceContainer">#EFEEED</Color>
    <Color x:Key="SurfaceContainerLow">#F4F3F2</Color>
    <Color x:Key="SurfaceContainerHigh">#E9E8E7</Color>
    <Color x:Key="SurfaceContainerHighest">#E3E2E1</Color>
    <Color x:Key="SurfaceContainerLowest">#FFFFFF</Color>
    <Color x:Key="SurfaceTint">#0060AB</Color>
    <Color x:Key="SurfaceVariant">#E3E2E1</Color>

    <!-- On Surface -->
    <Color x:Key="OnSurface">#1A1C1C</Color>
    <Color x:Key="OnSurfaceVariant">#404752</Color>
    <Color x:Key="OnBackground">#1A1C1C</Color>

    <!-- Outline -->
    <Color x:Key="Outline">#717783</Color>
    <Color x:Key="OutlineVariant">#C0C7D4</Color>

    <!-- Inverse -->
    <Color x:Key="InverseSurface">#2F3130</Color>
    <Color x:Key="InverseOnSurface">#F1F0EF</Color>
    <Color x:Key="InversePrimary">#A3C9FF</Color>

    <!-- Utility -->
    <Color x:Key="Background">#FAF9F8</Color>
    <Color x:Key="White">#FFFFFF</Color>
    <Color x:Key="Black">#000000</Color>

    <!-- SolidColorBrush references for binding in code -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource Primary}" />
    <SolidColorBrush x:Key="PrimaryContainerBrush" Color="{StaticResource PrimaryContainer}" />
    <SolidColorBrush x:Key="SecondaryContainerBrush" Color="{StaticResource SecondaryContainer}" />
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource Surface}" />
    <SolidColorBrush x:Key="SurfaceContainerBrush" Color="{StaticResource SurfaceContainer}" />
    <SolidColorBrush x:Key="SurfaceContainerLowBrush" Color="{StaticResource SurfaceContainerLow}" />
    <SolidColorBrush x:Key="SurfaceContainerLowestBrush" Color="{StaticResource SurfaceContainerLowest}" />
    <SolidColorBrush x:Key="OnSurfaceBrush" Color="{StaticResource OnSurface}" />
    <SolidColorBrush x:Key="OnSurfaceVariantBrush" Color="{StaticResource OnSurfaceVariant}" />
    <SolidColorBrush x:Key="OutlineVariantBrush" Color="{StaticResource OutlineVariant}" />
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource Error}" />

</ResourceDictionary>
```

- [ ] **Step 2: Update Android platform colors**

Replace `MinuteMind/Platforms/Android/Resources/values/colors.xml` with:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <color name="colorPrimary">#005FAA</color>
    <color name="colorPrimaryDark">#004883</color>
    <color name="colorAccent">#0078D4</color>
    <color name="colorPrimaryLight">#D3E3FF</color>
</resources>
```

- [ ] **Step 3: Build to verify no XAML errors**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/Resources/Styles/Colors.xaml MinuteMind/Platforms/Android/Resources/values/colors.xml
git commit -m "feat: rewrite Colors.xaml with M3 tonal palette from Stitch mockups"
```

---

### Task 2: Add Inter Font + Update Font Registration

**Files:**
- Modify: `MinuteMind/MauiProgram.cs`
- Note: Inter font files need to be downloaded and placed in `MinuteMind/Resources/Fonts/`

- [ ] **Step 1: Download Inter font files**

Download Inter Regular (400), Medium (500), and SemiBold (600) `.ttf` files from Google Fonts and place in `MinuteMind/Resources/Fonts/`:
- `Inter-Regular.ttf`
- `Inter-Medium.ttf`
- `Inter-SemiBold.ttf`

If wget/curl is available:
```bash
# Download from Google Fonts API or use the font files bundled in the mockup references
# Place files in MinuteMind/Resources/Fonts/
```

- [ ] **Step 2: Update MauiProgram.cs font registration**

In `MinuteMind/MauiProgram.cs`, replace the `ConfigureFonts` block with:

```csharp
.ConfigureFonts(fonts =>
{
    // Plus Jakarta Sans — Headlines
    fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaBold");
    fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakartaSemiBold");
    fonts.AddFont("PlusJakartaSans-Medium.ttf", "PlusJakartaMedium");
    fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaRegular");
    fonts.AddFont("PlusJakartaSans-Light.ttf", "PlusJakartaLight");

    // Inter — Body & Labels
    fonts.AddFont("Inter-Regular.ttf", "InterRegular");
    fonts.AddFont("Inter-Medium.ttf", "InterMedium");
    fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");

    // Material Symbols Outlined — Icons
    fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialIcons");
});
```

Note: The Material Symbols font file (`MaterialSymbolsOutlined-Regular.ttf`) also needs to be downloaded from Google Fonts and placed in `MinuteMind/Resources/Fonts/`. This enables using `FontImageSource` with glyph codes for all icons throughout the app. If this proves difficult to source, we will fall back to individual SVG icon files in `Resources/Images/`.

- [ ] **Step 3: Remove unused OpenSans fonts**

Delete `OpenSans-Regular.ttf` and `OpenSans-Semibold.ttf` from `MinuteMind/Resources/Fonts/` if they exist. They are no longer referenced.

- [ ] **Step 4: Build to verify fonts load**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded. (Missing font files will be warnings, not errors — ensure all .ttf files are present.)

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/Resources/Fonts/ MinuteMind/MauiProgram.cs
git commit -m "feat: add Inter + Material Symbols fonts, update font registration"
```

---

### Task 3: Update Styles.xaml with Design System Typography

**Files:**
- Modify: `MinuteMind/Resources/Styles/Styles.xaml`

- [ ] **Step 1: Add named typography styles to Styles.xaml**

Add these named styles inside the existing `<ResourceDictionary>` in `MinuteMind/Resources/Styles/Styles.xaml` (keep existing implicit styles, add these after them):

```xml
<!-- ============================================ -->
<!-- MinuteMind Typography Styles                 -->
<!-- ============================================ -->

<Style x:Key="DisplayLarge" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="34" />
    <Setter Property="CharacterSpacing" Value="-30" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
</Style>

<Style x:Key="HeadlineLarge" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="28" />
    <Setter Property="CharacterSpacing" Value="-30" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
</Style>

<Style x:Key="HeadlineMedium" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="24" />
    <Setter Property="CharacterSpacing" Value="-15" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
</Style>

<Style x:Key="HeadlineSmall" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="18" />
    <Setter Property="CharacterSpacing" Value="-15" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
</Style>

<Style x:Key="TitleMedium" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaSemiBold" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="CharacterSpacing" Value="-10" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
</Style>

<Style x:Key="BodyLarge" TargetType="Label">
    <Setter Property="FontFamily" Value="InterRegular" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="TextColor" Value="{StaticResource OnSurfaceVariant}" />
    <Setter Property="LineBreakMode" Value="WordWrap" />
</Style>

<Style x:Key="BodyMedium" TargetType="Label">
    <Setter Property="FontFamily" Value="InterRegular" />
    <Setter Property="FontSize" Value="14" />
    <Setter Property="TextColor" Value="{StaticResource OnSurfaceVariant}" />
    <Setter Property="LineBreakMode" Value="WordWrap" />
</Style>

<Style x:Key="BodySmall" TargetType="Label">
    <Setter Property="FontFamily" Value="InterRegular" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="TextColor" Value="{StaticResource OnSurfaceVariant}" />
</Style>

<Style x:Key="LabelSmall" TargetType="Label">
    <Setter Property="FontFamily" Value="InterSemiBold" />
    <Setter Property="FontSize" Value="10" />
    <Setter Property="CharacterSpacing" Value="80" />
    <Setter Property="TextTransform" Value="Uppercase" />
    <Setter Property="TextColor" Value="{StaticResource OnSurfaceVariant}" />
</Style>

<Style x:Key="LabelMedium" TargetType="Label">
    <Setter Property="FontFamily" Value="InterMedium" />
    <Setter Property="FontSize" Value="12" />
    <Setter Property="CharacterSpacing" Value="40" />
    <Setter Property="TextTransform" Value="Uppercase" />
    <Setter Property="TextColor" Value="{StaticResource OnSurfaceVariant}" />
</Style>

<Style x:Key="TimerDisplay" TargetType="Label">
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="64" />
    <Setter Property="CharacterSpacing" Value="-60" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    <Setter Property="HorizontalTextAlignment" Value="Center" />
</Style>

<!-- ============================================ -->
<!-- MinuteMind Button Styles                     -->
<!-- ============================================ -->

<Style x:Key="PrimaryButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource PrimaryContainer}" />
    <Setter Property="TextColor" Value="{StaticResource OnPrimaryContainer}" />
    <Setter Property="FontFamily" Value="PlusJakartaBold" />
    <Setter Property="FontSize" Value="15" />
    <Setter Property="CornerRadius" Value="9999" />
    <Setter Property="HeightRequest" Value="48" />
    <Setter Property="Padding" Value="24,0" />
</Style>

<Style x:Key="SecondaryButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource SurfaceContainerHigh}" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    <Setter Property="FontFamily" Value="PlusJakartaSemiBold" />
    <Setter Property="FontSize" Value="15" />
    <Setter Property="CornerRadius" Value="9999" />
    <Setter Property="HeightRequest" Value="48" />
    <Setter Property="Padding" Value="24,0" />
    <Setter Property="BorderWidth" Value="0" />
</Style>

<Style x:Key="OutlineButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{StaticResource Primary}" />
    <Setter Property="FontFamily" Value="PlusJakartaSemiBold" />
    <Setter Property="FontSize" Value="15" />
    <Setter Property="CornerRadius" Value="9999" />
    <Setter Property="HeightRequest" Value="48" />
    <Setter Property="Padding" Value="24,0" />
    <Setter Property="BorderColor" Value="{StaticResource OutlineVariant}" />
    <Setter Property="BorderWidth" Value="1" />
</Style>
```

- [ ] **Step 2: Update the implicit Page style for Surface background**

Find the existing implicit `Style TargetType="Page"` in Styles.xaml and ensure it sets:

```xml
<Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
```

Also update the implicit `Label` style to default to InterRegular + OnSurface:

```xml
<Style TargetType="Label">
    <Setter Property="FontFamily" Value="InterRegular" />
    <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    <Setter Property="FontSize" Value="14" />
</Style>
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/Resources/Styles/Styles.xaml
git commit -m "feat: add typography and button styles matching design system"
```

---

### Task 4: Setup AppShell with 4-Tab Navigation

**Files:**
- Modify: `MinuteMind/AppShell.xaml`
- Modify: `MinuteMind/AppShell.xaml.cs`
- Create: `MinuteMind/Views/RecordingPage.xaml` (stub)
- Create: `MinuteMind/Views/RecordingPage.xaml.cs` (stub)
- Create: `MinuteMind/Views/MeetingsPage.xaml` (stub)
- Create: `MinuteMind/Views/MeetingsPage.xaml.cs` (stub)
- Create: `MinuteMind/Views/SettingsPage.xaml` (stub)
- Create: `MinuteMind/Views/SettingsPage.xaml.cs` (stub)

- [ ] **Step 1: Create stub pages for all 4 tab roots**

Create `MinuteMind/Views/RecordingPage.xaml`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MinuteMind.Views.RecordingPage"
             Title="Record">
    <Label Text="Recording Page" Style="{StaticResource HeadlineLarge}" VerticalOptions="Center" HorizontalOptions="Center" />
</ContentPage>
```

Create `MinuteMind/Views/RecordingPage.xaml.cs`:
```csharp
namespace MinuteMind.Views;

public partial class RecordingPage : ContentPage
{
    public RecordingPage()
    {
        InitializeComponent();
    }
}
```

Create `MinuteMind/Views/MeetingsPage.xaml`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MinuteMind.Views.MeetingsPage"
             Title="Search">
    <Label Text="Meetings Page" Style="{StaticResource HeadlineLarge}" VerticalOptions="Center" HorizontalOptions="Center" />
</ContentPage>
```

Create `MinuteMind/Views/MeetingsPage.xaml.cs`:
```csharp
namespace MinuteMind.Views;

public partial class MeetingsPage : ContentPage
{
    public MeetingsPage()
    {
        InitializeComponent();
    }
}
```

Create `MinuteMind/Views/SettingsPage.xaml`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MinuteMind.Views.SettingsPage"
             Title="Settings">
    <Label Text="Settings Page" Style="{StaticResource HeadlineLarge}" VerticalOptions="Center" HorizontalOptions="Center" />
</ContentPage>
```

Create `MinuteMind/Views/SettingsPage.xaml.cs`:
```csharp
namespace MinuteMind.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 2: Rewrite AppShell.xaml with 4-tab TabBar**

Replace `MinuteMind/AppShell.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="MinuteMind.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:MinuteMind.Views"
    Shell.TabBarBackgroundColor="{StaticResource SurfaceContainerLowest}"
    Shell.TabBarForegroundColor="{StaticResource Primary}"
    Shell.TabBarUnselectedColor="#661A1C1C"
    Shell.TabBarTitleColor="{StaticResource Primary}">

    <TabBar>
        <ShellContent
            Title="MEETINGS"
            Icon="tab_meetings.png"
            ContentTemplate="{DataTemplate views:Dashboard}"
            Route="Dashboard" />

        <ShellContent
            Title="RECORD"
            Icon="tab_record.png"
            ContentTemplate="{DataTemplate views:RecordingPage}"
            Route="RecordingPage" />

        <ShellContent
            Title="SEARCH"
            Icon="tab_search.png"
            ContentTemplate="{DataTemplate views:MeetingsPage}"
            Route="MeetingsPage" />

        <ShellContent
            Title="SETTINGS"
            Icon="tab_settings.png"
            ContentTemplate="{DataTemplate views:SettingsPage}"
            Route="SettingsPage" />
    </TabBar>

</Shell>
```

Note: Tab icons (`tab_meetings.png`, `tab_record.png`, `tab_search.png`, `tab_settings.png`) need to be created as SVG files in `MinuteMind/Resources/Images/` and referenced. For now, these can be simple placeholder PNGs or we can use `FontImageSource` in the code-behind instead.

- [ ] **Step 3: Update AppShell.xaml.cs to register routes**

Replace `MinuteMind/AppShell.xaml.cs`:

```csharp
namespace MinuteMind;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register non-tab routes for push navigation
        Routing.RegisterRoute(nameof(Views.ProcessingPage), typeof(Views.ProcessingPage));
        Routing.RegisterRoute(nameof(Views.TranscriptPage), typeof(Views.TranscriptPage));
        Routing.RegisterRoute(nameof(Views.MinutesPage), typeof(Views.MinutesPage));
        Routing.RegisterRoute(nameof(Views.EditMinutesPage), typeof(Views.EditMinutesPage));
        Routing.RegisterRoute(nameof(Views.ExportPage), typeof(Views.ExportPage));
    }
}
```

- [ ] **Step 4: Create stub pages for push-navigation screens**

Create these minimal stub pages (same pattern as step 1):

`MinuteMind/Views/ProcessingPage.xaml` + `.cs` — Title="Processing"
`MinuteMind/Views/TranscriptPage.xaml` + `.cs` — Title="Transcript"
`MinuteMind/Views/MinutesPage.xaml` + `.cs` — Title="Minutes"
`MinuteMind/Views/EditMinutesPage.xaml` + `.cs` — Title="Edit Minutes"
`MinuteMind/Views/ExportPage.xaml` + `.cs` — Title="Export"

Each follows the same stub pattern:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MinuteMind.Views.{PageName}"
             Title="{Title}">
    <Label Text="{PageName}" Style="{StaticResource HeadlineLarge}" VerticalOptions="Center" HorizontalOptions="Center" />
</ContentPage>
```

```csharp
namespace MinuteMind.Views;

public partial class {PageName} : ContentPage
{
    public {PageName}()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 5: Create placeholder tab icons**

Create simple SVG icons in `MinuteMind/Resources/Images/`:
- `tab_meetings.svg` — document icon
- `tab_record.svg` — microphone icon
- `tab_search.svg` — magnifying glass icon
- `tab_settings.svg` — gear icon

Or alternatively, switch to `FontImageSource` in AppShell.xaml.cs if using Material Symbols font. Either approach works — pick whichever is simpler to get running.

- [ ] **Step 6: Delete old MainPage.xaml**

Remove `MinuteMind/MainPage.xaml` and `MinuteMind/MainPage.xaml.cs` if they still exist (they were already deleted per git status).

- [ ] **Step 7: Build and verify tabs render**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded. App should show 4 tabs at bottom with stub content.

- [ ] **Step 8: Commit**

```bash
git add MinuteMind/AppShell.xaml MinuteMind/AppShell.xaml.cs MinuteMind/Views/ MinuteMind/Resources/Images/
git commit -m "feat: setup 4-tab Shell navigation with stub pages for all 9 screens"
```

---

## Phase 2: Infrastructure (Models, Database, Services, DI)

### Task 5: Create Data Models

**Files:**
- Create: `MinuteMind/Models/Meeting.cs`
- Create: `MinuteMind/Models/MeetingStatus.cs`
- Create: `MinuteMind/Models/TranscriptSegment.cs`
- Create: `MinuteMind/Models/AiInsightBreak.cs`
- Create: `MinuteMind/Models/MeetingMinutes.cs`
- Create: `MinuteMind/Models/Decision.cs`
- Create: `MinuteMind/Models/ActionItem.cs`
- Create: `MinuteMind/Models/ProcessingStep.cs`

- [ ] **Step 1: Create MeetingStatus enum**

Create `MinuteMind/Models/MeetingStatus.cs`:
```csharp
namespace MinuteMind.Models;

public enum MeetingStatus
{
    Recording,
    Processing,
    Transcribed,
    MinutesReady
}
```

- [ ] **Step 2: Create Meeting model**

Create `MinuteMind/Models/Meeting.cs`:
```csharp
using SQLite;

namespace MinuteMind.Models;

public class Meeting
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MeetingType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long DurationTicks { get; set; }
    public MeetingStatus Status { get; set; }
    public string? AudioFilePath { get; set; }
    public string? TranscriptJson { get; set; }
    public string? MinutesJson { get; set; }
    public string? HtmlContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [Ignore]
    public TimeSpan Duration
    {
        get => TimeSpan.FromTicks(DurationTicks);
        set => DurationTicks = value.Ticks;
    }
}
```

Note: SQLite doesn't natively store `TimeSpan`, so we use `DurationTicks` as the stored column and `Duration` as a computed `[Ignore]` property.

- [ ] **Step 3: Create transcript and minutes models**

Create `MinuteMind/Models/TranscriptSegment.cs`:
```csharp
namespace MinuteMind.Models;

public class TranscriptSegment
{
    public TimeSpan Timestamp { get; set; }
    public string Speaker { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
```

Create `MinuteMind/Models/AiInsightBreak.cs`:
```csharp
namespace MinuteMind.Models;

public class AiInsightBreak
{
    public string Topic { get; set; } = string.Empty;
}
```

Create `MinuteMind/Models/Decision.cs`:
```csharp
namespace MinuteMind.Models;

public class Decision
{
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
```

Create `MinuteMind/Models/ActionItem.cs`:
```csharp
namespace MinuteMind.Models;

public class ActionItem
{
    public string Description { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
```

Create `MinuteMind/Models/MeetingMinutes.cs`:
```csharp
namespace MinuteMind.Models;

public class MeetingMinutes
{
    public List<string> Attendees { get; set; } = [];
    public List<string> DiscussionPoints { get; set; } = [];
    public List<Decision> Decisions { get; set; } = [];
    public List<ActionItem> ActionItems { get; set; } = [];
}
```

- [ ] **Step 4: Create ProcessingStep model**

Create `MinuteMind/Models/ProcessingStep.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MinuteMind.Models;

public enum StepStatus { Pending, Active, Completed }

public partial class ProcessingStep : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private StepStatus _status = StepStatus.Pending;
}
```

- [ ] **Step 5: Build to verify models compile**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add MinuteMind/Models/
git commit -m "feat: add data models — Meeting, Transcript, Minutes, ActionItem, ProcessingStep"
```

---

### Task 6: Setup SQLite Database

**Files:**
- Create: `MinuteMind/Data/MinuteMindDatabase.cs`

- [ ] **Step 1: Create the database class**

Create `MinuteMind/Data/MinuteMindDatabase.cs`:
```csharp
using MinuteMind.Models;
using SQLite;

namespace MinuteMind.Data;

public class MinuteMindDatabase
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;

    public MinuteMindDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "minutemind.db3");
    }

    private async Task InitAsync()
    {
        if (_db is not null)
            return;

        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Meeting>();
    }

    public async Task<List<Meeting>> GetAllMeetingsAsync()
    {
        await InitAsync();
        return await _db!.Table<Meeting>().OrderByDescending(m => m.Date).ToListAsync();
    }

    public async Task<List<Meeting>> GetRecentMeetingsAsync(int count)
    {
        await InitAsync();
        return await _db!.Table<Meeting>().OrderByDescending(m => m.Date).Take(count).ToListAsync();
    }

    public async Task<Meeting?> GetMeetingAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<Meeting>().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> SaveMeetingAsync(Meeting meeting)
    {
        await InitAsync();
        meeting.UpdatedAt = DateTime.UtcNow;

        if (meeting.Id == 0)
        {
            meeting.CreatedAt = DateTime.UtcNow;
            return await _db!.InsertAsync(meeting);
        }

        return await _db!.UpdateAsync(meeting);
    }

    public async Task<int> DeleteMeetingAsync(Meeting meeting)
    {
        await InitAsync();
        return await _db!.DeleteAsync(meeting);
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add MinuteMind/Data/MinuteMindDatabase.cs
git commit -m "feat: add SQLite database with Meeting CRUD operations"
```

---

### Task 7: Create Service Interfaces

**Files:**
- Modify: `MinuteMind/Services/Contracts/INavigationService.cs` (already exists)
- Create: `MinuteMind/Services/Contracts/IAudioRecorderService.cs`
- Create: `MinuteMind/Services/Contracts/ITranscriptionService.cs`
- Create: `MinuteMind/Services/Contracts/IMinutesGeneratorService.cs`
- Create: `MinuteMind/Services/Contracts/IMeetingRepository.cs`
- Create: `MinuteMind/Services/Contracts/IPdfExportService.cs`

- [ ] **Step 1: Create all service interfaces**

Create `MinuteMind/Services/Contracts/IAudioRecorderService.cs`:
```csharp
namespace MinuteMind.Services.Contracts;

public interface IAudioRecorderService
{
    Task<string> StartAsync();
    Task PauseAsync();
    Task ResumeAsync();
    Task<string> StopAsync();
    bool IsRecording { get; }
    bool IsPaused { get; }
}
```

Create `MinuteMind/Services/Contracts/ITranscriptionService.cs`:
```csharp
using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface ITranscriptionService
{
    Task<List<TranscriptSegment>> TranscribeAsync(string audioPath, IProgress<string>? progress = null);
}
```

Create `MinuteMind/Services/Contracts/IMinutesGeneratorService.cs`:
```csharp
using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IMinutesGeneratorService
{
    Task<MeetingMinutes> GenerateAsync(List<TranscriptSegment> transcript);
}
```

Create `MinuteMind/Services/Contracts/IMeetingRepository.cs`:
```csharp
using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IMeetingRepository
{
    Task<List<Meeting>> GetAllAsync();
    Task<List<Meeting>> GetRecentAsync(int count);
    Task<Meeting?> GetByIdAsync(int id);
    Task<int> SaveAsync(Meeting meeting);
    Task<int> DeleteAsync(Meeting meeting);
}
```

Create `MinuteMind/Services/Contracts/IPdfExportService.cs`:
```csharp
using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IPdfExportService
{
    Task<string> ExportAsync(Meeting meeting);
}
```

- [ ] **Step 2: Commit**

```bash
git add MinuteMind/Services/Contracts/
git commit -m "feat: add service interfaces — audio, transcription, minutes, repository, PDF"
```

---

### Task 8: Implement Service Implementations

**Files:**
- Create: `MinuteMind/Services/Implementations/MeetingRepository.cs`
- Create: `MinuteMind/Services/Implementations/MockMinutesGeneratorService.cs`
- Create: `MinuteMind/Services/Implementations/AudioRecorderService.cs` (stub)
- Create: `MinuteMind/Services/Implementations/LocalTranscriptionService.cs` (stub)
- Create: `MinuteMind/Services/Implementations/PdfExportService.cs` (stub)

- [ ] **Step 1: Create MeetingRepository (wraps MinuteMindDatabase)**

Create `MinuteMind/Services/Implementations/MeetingRepository.cs`:
```csharp
using MinuteMind.Data;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class MeetingRepository(MinuteMindDatabase database) : IMeetingRepository
{
    public Task<List<Meeting>> GetAllAsync() => database.GetAllMeetingsAsync();
    public Task<List<Meeting>> GetRecentAsync(int count) => database.GetRecentMeetingsAsync(count);
    public Task<Meeting?> GetByIdAsync(int id) => database.GetMeetingAsync(id);
    public Task<int> SaveAsync(Meeting meeting) => database.SaveMeetingAsync(meeting);
    public Task<int> DeleteAsync(Meeting meeting) => database.DeleteMeetingAsync(meeting);
}
```

- [ ] **Step 2: Create MockMinutesGeneratorService**

Create `MinuteMind/Services/Implementations/MockMinutesGeneratorService.cs`:
```csharp
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class MockMinutesGeneratorService : IMinutesGeneratorService
{
    public async Task<MeetingMinutes> GenerateAsync(List<TranscriptSegment> transcript)
    {
        // Simulate AI processing delay
        await Task.Delay(2000);

        return new MeetingMinutes
        {
            Attendees = ["John Doe", "Mary Smith", "Alex Rivera"],
            DiscussionPoints =
            [
                "Review of the Q4 roadmap and alignment on technical debt prioritization for the upcoming sprint cycles.",
                "Detailed analysis of the current conversion funnel drop-off at the payment gateway stage.",
                "Initial brainstorming for the 2024 mobile redesign concept, focusing on accessibility and speed."
            ],
            Decisions =
            [
                new Decision
                {
                    Category = "Technical",
                    Text = "Approved the shift to a micro-services architecture for the checkout flow by Q1."
                },
                new Decision
                {
                    Category = "Budgeting",
                    Text = "Allocated 15% of engineering capacity to address high-priority security vulnerabilities."
                }
            ],
            ActionItems =
            [
                new ActionItem
                {
                    Description = "Finalize the architecture diagram",
                    Assignee = "John",
                    DueDate = "Oct 27",
                    IsCompleted = false
                },
                new ActionItem
                {
                    Description = "Schedule follow-up with DevOps",
                    Assignee = "Mary",
                    DueDate = "Completed",
                    IsCompleted = true
                },
                new ActionItem
                {
                    Description = "Update Jira board with new priorities",
                    Assignee = "Alex",
                    DueDate = "Tomorrow",
                    IsCompleted = false
                },
                new ActionItem
                {
                    Description = "Prepare Q4 roadmap presentation",
                    Assignee = "Alex",
                    DueDate = "Friday",
                    IsCompleted = false
                }
            ]
        };
    }
}
```

- [ ] **Step 3: Create AudioRecorderService stub**

Create `MinuteMind/Services/Implementations/AudioRecorderService.cs`:
```csharp
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class AudioRecorderService : IAudioRecorderService
{
    public bool IsRecording { get; private set; }
    public bool IsPaused { get; private set; }

    public Task<string> StartAsync()
    {
        IsRecording = true;
        IsPaused = false;
        // TODO: Wire up Plugin.Maui.Audio when NuGet is added
        return Task.FromResult(string.Empty);
    }

    public Task PauseAsync()
    {
        IsPaused = true;
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        IsPaused = false;
        return Task.CompletedTask;
    }

    public Task<string> StopAsync()
    {
        IsRecording = false;
        IsPaused = false;
        var path = Path.Combine(FileSystem.AppDataDirectory, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
        // TODO: Return actual recorded file path
        return Task.FromResult(path);
    }
}
```

- [ ] **Step 4: Create LocalTranscriptionService stub**

Create `MinuteMind/Services/Implementations/LocalTranscriptionService.cs`:
```csharp
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class LocalTranscriptionService : ITranscriptionService
{
    public async Task<List<TranscriptSegment>> TranscribeAsync(string audioPath, IProgress<string>? progress = null)
    {
        // TODO: Load Whisper model from Resources/Raw/ and run real inference
        // For now, return sample data with simulated delay

        progress?.Report("Loading model...");
        await Task.Delay(1000);

        progress?.Report("Decoding speaker 1...");
        await Task.Delay(1500);

        progress?.Report("Decoding speaker 2...");
        await Task.Delay(1500);

        return
        [
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromSeconds(4),
                Speaker = "Speaker 1",
                Text = "Alright everyone, let's dive into the Q3 product roadmap. We've seen significant traction with the new typography system. How is the feedback from the mobile dev team looking so far?"
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromSeconds(48),
                Speaker = "Speaker 2",
                Text = "The team is loving the Plus Jakarta Sans implementation. It gives that editorial edge we were aiming for. However, we're seeing some layout shifting on smaller devices when the headlines wrap."
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(22),
                Speaker = "Speaker 1",
                Text = "That's a fair point. We should probably adjust the tracking for those specific breakpoints. Can we look at the fluid typography scale again? I want to make sure the \"Digital Architect\" vibe isn't lost on iPhone Mini users."
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(15),
                Speaker = "Speaker 2",
                Text = "Absolutely. I'll sync with the UI developers this afternoon. Beyond that, the performance metrics for the transcription engine are peaking at 98% accuracy, which is phenomenal."
            }
        ];
    }
}
```

- [ ] **Step 5: Create PdfExportService stub**

Create `MinuteMind/Services/Implementations/PdfExportService.cs`:
```csharp
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    public async Task<string> ExportAsync(Meeting meeting)
    {
        // TODO: Implement Syncfusion PDF generation
        await Task.Delay(500);
        var path = Path.Combine(FileSystem.AppDataDirectory, $"{meeting.Title.Replace(" ", "_")}.pdf");
        // Placeholder — write empty file
        await File.WriteAllBytesAsync(path, []);
        return path;
    }
}
```

- [ ] **Step 6: Build to verify**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded.

- [ ] **Step 7: Commit**

```bash
git add MinuteMind/Services/Implementations/
git commit -m "feat: add service implementations — repository, mock minutes, audio/transcription/PDF stubs"
```

---

### Task 9: Wire Up Dependency Injection in MauiProgram.cs

**Files:**
- Modify: `MinuteMind/MauiProgram.cs`

- [ ] **Step 1: Add all DI registrations**

Replace the contents of `MinuteMind/MauiProgram.cs` with:

```csharp
using CommunityToolkit.Maui;
using DotNetMeteor.HotReload.Plugin;
using MinuteMind.Data;
using MinuteMind.Services.Contracts;
using MinuteMind.Services.Implementations;
using MinuteMind.ViewModels;
using MinuteMind.Views;

namespace MinuteMind;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .EnableHotReload()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaBold");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "PlusJakartaMedium");
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaRegular");
                fonts.AddFont("PlusJakartaSans-Light.ttf", "PlusJakartaLight");
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            });

        // Database
        builder.Services.AddSingleton<MinuteMindDatabase>();

        // Services
        builder.Services.AddSingleton<IMeetingRepository, MeetingRepository>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IAudioRecorderService, AudioRecorderService>();
        builder.Services.AddSingleton<ITranscriptionService, LocalTranscriptionService>();
        builder.Services.AddTransient<IMinutesGeneratorService, MockMinutesGeneratorService>();
        builder.Services.AddTransient<IPdfExportService, PdfExportService>();

        // ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<RecordingViewModel>();
        builder.Services.AddTransient<ProcessingViewModel>();
        builder.Services.AddTransient<TranscriptViewModel>();
        builder.Services.AddTransient<MinutesViewModel>();
        builder.Services.AddTransient<EditMinutesViewModel>();
        builder.Services.AddTransient<ExportViewModel>();
        builder.Services.AddTransient<MeetingsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<Dashboard>();
        builder.Services.AddTransient<RecordingPage>();
        builder.Services.AddTransient<ProcessingPage>();
        builder.Services.AddTransient<TranscriptPage>();
        builder.Services.AddTransient<MinutesPage>();
        builder.Services.AddTransient<EditMinutesPage>();
        builder.Services.AddTransient<ExportPage>();
        builder.Services.AddTransient<MeetingsPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
```

Note: This will NOT compile yet — the ViewModel classes don't exist. That's expected. We create them in the next task.

- [ ] **Step 2: Commit (even if build fails — DI is correct, VMs coming next)**

```bash
git add MinuteMind/MauiProgram.cs
git commit -m "feat: wire up dependency injection for all services, ViewModels, and pages"
```

---

### Task 10: Create ViewModel Stubs

**Files:**
- Create: `MinuteMind/ViewModels/DashboardViewModel.cs`
- Create: `MinuteMind/ViewModels/RecordingViewModel.cs`
- Create: `MinuteMind/ViewModels/ProcessingViewModel.cs`
- Create: `MinuteMind/ViewModels/TranscriptViewModel.cs`
- Create: `MinuteMind/ViewModels/MinutesViewModel.cs`
- Create: `MinuteMind/ViewModels/EditMinutesViewModel.cs`
- Create: `MinuteMind/ViewModels/ExportViewModel.cs`
- Create: `MinuteMind/ViewModels/MeetingsViewModel.cs`
- Create: `MinuteMind/ViewModels/SettingsViewModel.cs`

- [ ] **Step 1: Create all ViewModel stubs**

Each ViewModel follows this pattern — a minimal `ObservableObject` subclass with constructor injection. We'll flesh them out when building each screen.

Create `MinuteMind/ViewModels/DashboardViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class DashboardViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/RecordingViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class RecordingViewModel(
    IAudioRecorderService audioRecorder,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/ProcessingViewModel.cs`:
```csharp
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
```

Create `MinuteMind/ViewModels/TranscriptViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class TranscriptViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/MinutesViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/EditMinutesViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class EditMinutesViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/ExportViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class ExportViewModel(
    IMeetingRepository meetingRepository,
    IPdfExportService pdfExportService,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/MeetingsViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MinuteMind.Services.Contracts;

namespace MinuteMind.ViewModels;

public partial class MeetingsViewModel(
    IMeetingRepository meetingRepository,
    INavigationService navigationService) : ObservableObject
{
}
```

Create `MinuteMind/ViewModels/SettingsViewModel.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MinuteMind.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
}
```

- [ ] **Step 2: Wire ViewModels into stub pages via constructor injection**

Update each stub page's code-behind to accept its ViewModel via constructor injection and set `BindingContext`. For example, update `MinuteMind/Views/Dashboard.xaml.cs`:

```csharp
using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class Dashboard : ContentPage
{
    public Dashboard(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

Repeat this pattern for ALL 9 pages:
- `Dashboard` ← `DashboardViewModel`
- `RecordingPage` ← `RecordingViewModel`
- `ProcessingPage` ← `ProcessingViewModel`
- `TranscriptPage` ← `TranscriptViewModel`
- `MinutesPage` ← `MinutesViewModel`
- `EditMinutesPage` ← `EditMinutesViewModel`
- `ExportPage` ← `ExportViewModel`
- `MeetingsPage` ← `MeetingsViewModel`
- `SettingsPage` ← `SettingsViewModel`

- [ ] **Step 3: Build to verify everything compiles**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android --no-restore`
Expected: Build succeeded. Full DI chain is working — database → services → ViewModels → pages.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/ MinuteMind/Views/
git commit -m "feat: add ViewModel stubs with DI, wire into all pages"
```

---

## Phase 3: Build Screens (one at a time)

### Task 11: Build Dashboard Screen (Home Tab)

**Files:**
- Modify: `MinuteMind/ViewModels/DashboardViewModel.cs`
- Modify: `MinuteMind/Views/Dashboard.xaml`

- [ ] **Step 1: Implement DashboardViewModel**

Replace `MinuteMind/ViewModels/DashboardViewModel.cs`:

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
    public ObservableCollection<Meeting> RecentMeetings { get; } = [];

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var meetings = await meetingRepository.GetRecentAsync(5);
        RecentMeetings.Clear();
        foreach (var meeting in meetings)
            RecentMeetings.Add(meeting);
    }

    [RelayCommand]
    private async Task RecordAsync()
    {
        await navigationService.GoToAsync("//RecordingPage");
    }

    [RelayCommand]
    private async Task UploadAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Meeting Audio",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/x-m4a", "audio/mp4" } },
                { DevicePlatform.iOS, new[] { "public.mp3", "public.wav", "com.apple.m4a-audio" } },
            })
        });

        if (result is null)
            return;

        await navigationService.GoToAsync(nameof(Views.ProcessingPage),
            new Dictionary<string, object> { { "AudioFilePath", result.FullPath } });
    }

    [RelayCommand]
    private async Task ViewMeetingAsync(Meeting meeting)
    {
        await navigationService.GoToAsync(nameof(Views.MinutesPage),
            new Dictionary<string, object> { { "MeetingId", meeting.Id } });
    }

    [RelayCommand]
    private async Task ViewAllAsync()
    {
        await navigationService.GoToAsync("//MeetingsPage");
    }
}
```

- [ ] **Step 2: Build the Dashboard XAML**

Replace `MinuteMind/Views/Dashboard.xaml` with the full layout matching the Stitch mockup. This is the largest XAML file — it includes:

- Header (hamburger + MinuteMind title + avatar)
- Hero section ("Capture Clarity.")
- 2-column bento action cards (Record Meeting + Upload Audio)
- Recent Meetings CollectionView with meeting card DataTemplate
- AI Pulse footer

The XAML should reference the styles from Task 3 (`DisplayLarge`, `HeadlineSmall`, `BodyMedium`, `LabelSmall`, etc.) and colors from Task 1. Use `Border` elements with `StrokeShape="RoundRectangle"` for rounded cards. Use `Shadow` for the ghost shadow effect.

Key bindings:
- `CollectionView ItemsSource="{Binding RecentMeetings}"`
- Record card: `TapGestureRecognizer Command="{Binding RecordCommand}"`
- Upload card: `TapGestureRecognizer Command="{Binding UploadCommand}"`
- Each meeting card: `TapGestureRecognizer Command="{Binding ViewMeetingCommand}" CommandParameter="{Binding .}"`
- `Page.Behaviors` with `EventToCommandBehavior` for `Appearing` → `LoadDataCommand`

Refer to `MinuteMind/mockups/home_screen/code.html` for exact layout structure, spacing, and visual hierarchy.

- [ ] **Step 3: Create a MeetingStatusToStringConverter**

Create `MinuteMind/Converters/MeetingStatusToStringConverter.cs`:
```csharp
using System.Globalization;
using MinuteMind.Models;

namespace MinuteMind.Converters;

public class MeetingStatusToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is MeetingStatus status ? status switch
        {
            MeetingStatus.Recording => "RECORDING",
            MeetingStatus.Processing => "PROCESSING",
            MeetingStatus.Transcribed => "TRANSCRIBED",
            MeetingStatus.MinutesReady => "MINUTES READY",
            _ => string.Empty
        } : string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 4: Build and verify on emulator/device**

Run: `dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android`
Deploy to emulator and verify:
- 4 tabs visible at bottom
- Dashboard shows header, hero, action cards, empty "Recent Meetings" (no data yet)
- Tapping Record switches to Record tab
- Tapping Upload opens file picker

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/ViewModels/DashboardViewModel.cs MinuteMind/Views/Dashboard.xaml MinuteMind/Converters/
git commit -m "feat: build Dashboard screen — hero, action cards, recent meetings list"
```

---

### Task 12: Build Recording Screen

**Files:**
- Modify: `MinuteMind/ViewModels/RecordingViewModel.cs`
- Modify: `MinuteMind/Views/RecordingPage.xaml`
- Create: `MinuteMind/Controls/WaveformView.cs`
- Create: `MinuteMind/Controls/WaveformDrawable.cs`

- [ ] **Step 1: Create WaveformView custom control**

Create `MinuteMind/Controls/WaveformDrawable.cs`:
```csharp
namespace MinuteMind.Controls;

public class WaveformDrawable : IDrawable
{
    public float[] Levels { get; set; } = new float[13];
    public Color BarColor { get; set; } = Color.FromArgb("#005FAA");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var barWidth = 6f;
        var gap = 6f;
        var totalWidth = Levels.Length * (barWidth + gap) - gap;
        var startX = (dirtyRect.Width - totalWidth) / 2;
        var maxHeight = dirtyRect.Height;

        for (var i = 0; i < Levels.Length; i++)
        {
            var level = Math.Clamp(Levels[i], 0.1f, 1.0f);
            var barHeight = maxHeight * level;
            var x = startX + i * (barWidth + gap);
            var y = (maxHeight - barHeight) / 2;

            canvas.FillColor = BarColor.WithAlpha(0.2f + level * 0.8f);
            canvas.FillRoundedRectangle(x, y, barWidth, barHeight, barWidth / 2);
        }
    }
}
```

Create `MinuteMind/Controls/WaveformView.cs`:
```csharp
namespace MinuteMind.Controls;

public class WaveformView : GraphicsView
{
    private readonly WaveformDrawable _drawable = new();

    public static readonly BindableProperty LevelsProperty =
        BindableProperty.Create(nameof(Levels), typeof(float[]), typeof(WaveformView),
            new float[13], propertyChanged: OnLevelsChanged);

    public float[] Levels
    {
        get => (float[])GetValue(LevelsProperty);
        set => SetValue(LevelsProperty, value);
    }

    public WaveformView()
    {
        Drawable = _drawable;
    }

    private static void OnLevelsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is WaveformView view && newValue is float[] levels)
        {
            view._drawable.Levels = levels;
            view.Invalidate();
        }
    }
}
```

- [ ] **Step 2: Implement RecordingViewModel**

Replace `MinuteMind/ViewModels/RecordingViewModel.cs` with full implementation:
- `IsRecording`, `IsPaused` observable properties
- `ElapsedTime` TimeSpan with DispatcherTimer incrementing every second
- `WaveformLevels` float[] with DispatcherTimer randomizing every 150ms
- `LiveInsightText` showing mock transcript snippets
- `StartCommand`, `PauseCommand`, `StopCommand` (Stop navigates to ProcessingPage with audio path)
- `MeetingTitle` default "Strategy Alignment Meeting"
- Timer formatting via string.Format("{0:hh\\:mm\\:ss}", ElapsedTime)

- [ ] **Step 3: Build RecordingPage.xaml**

Layout matching `MinuteMind/mockups/recording_screen/code.html`:
- Shell.TabBarIsVisible="False"
- AI Transcription Active chip
- Meeting title + metadata
- WaveformView (HeightRequest="128", bound to WaveformLevels)
- Timer display (TimerDisplay style)
- Pulsing stop button (red square inside white circle)
- Pause + Stop Recording buttons
- "Capturing from Internal Mic" toast at bottom
- Live Insight card

- [ ] **Step 4: Build and verify**

Expected: Recording screen shows with animated waveform, ticking timer, functional pause/stop buttons.

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/Controls/ MinuteMind/ViewModels/RecordingViewModel.cs MinuteMind/Views/RecordingPage.xaml
git commit -m "feat: build Recording screen with waveform visualization and timer"
```

---

### Task 13: Build Processing Screen

**Files:**
- Modify: `MinuteMind/ViewModels/ProcessingViewModel.cs`
- Modify: `MinuteMind/Views/ProcessingPage.xaml`

- [ ] **Step 1: Implement ProcessingViewModel**

Full implementation with:
- `Steps` ObservableCollection<ProcessingStep> (3 items: Upload, Transcribe, Generate)
- Receives `AudioFilePath` via `IQueryAttributable`
- `StartProcessingAsync()` called on appearing:
  - Step 1: Complete immediately (audio already saved)
  - Step 2: Call `ITranscriptionService.TranscribeAsync()` with progress callback
  - Step 3: Call `IMinutesGeneratorService.GenerateAsync()`
  - Save meeting to SQLite, navigate to TranscriptPage

- [ ] **Step 2: Build ProcessingPage.xaml**

Layout matching `MinuteMind/mockups/processing_screen/code.html`:
- Shell.TabBarIsVisible="False", Shell.BackButtonBehavior (disabled)
- "Processing Meeting..." headline
- Central spinning ring (animated Border with RotateTo)
- 3 step cards with BindableLayout bound to Steps collection
- DataTriggers on StepStatus for visual state (completed/active/pending)
- Footer text

- [ ] **Step 3: Build and verify**

Expected: Processing screen auto-advances through 3 steps, then navigates to Transcript.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/ProcessingViewModel.cs MinuteMind/Views/ProcessingPage.xaml
git commit -m "feat: build Processing screen with animated step progression"
```

---

### Task 14: Build Transcript Screen

**Files:**
- Modify: `MinuteMind/ViewModels/TranscriptViewModel.cs`
- Modify: `MinuteMind/Views/TranscriptPage.xaml`
- Create: `MinuteMind/Views/TranscriptTemplateSelector.cs`

- [ ] **Step 1: Create ITranscriptItem interface and template selector**

Create a marker interface or base class so the CollectionView can hold both `TranscriptSegment` and `AiInsightBreak` items. Create a `DataTemplateSelector` that picks the right template.

- [ ] **Step 2: Implement TranscriptViewModel**

Receives `MeetingId` via `IQueryAttributable`, loads meeting from SQLite, deserializes `TranscriptJson` into mixed list of segments + AI insight breaks.

- [ ] **Step 3: Build TranscriptPage.xaml**

Layout matching `MinuteMind/mockups/transcript_screen/code.html`:
- Back button header
- Meeting info section (title, avatars, duration)
- CollectionView with DataTemplateSelector (speaker cards + AI insight chips)
- Floating "Generate Minutes" FAB at bottom

- [ ] **Step 4: Build and verify**

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/ViewModels/TranscriptViewModel.cs MinuteMind/Views/TranscriptPage.xaml MinuteMind/Views/TranscriptTemplateSelector.cs
git commit -m "feat: build Transcript screen with speaker segments and AI insight breaks"
```

---

### Task 15: Build Minutes Screen

**Files:**
- Modify: `MinuteMind/ViewModels/MinutesViewModel.cs`
- Modify: `MinuteMind/Views/MinutesPage.xaml`

- [ ] **Step 1: Implement MinutesViewModel**

Receives `MeetingId`, loads meeting, deserializes `MinutesJson`. Exposes attendees, discussion points, decisions, action items. Commands for Edit and Export navigation. Toggle action item command.

- [ ] **Step 2: Build MinutesPage.xaml**

Layout matching `MinuteMind/mockups/minutes_screen/code.html`:
- Hero section with blue left border
- Attendees card (white, rounded)
- Key Discussion Points card
- Decisions card (full primary-blue background with frosted sub-cards)
- Action Items section with checkboxes
- AI-Generated Intelligence Layer badge
- Bottom tab bar visible

This is the most visually complex screen. Take care with the Decisions card — it's a `Border` with `BackgroundColor="{StaticResource Primary}"` containing sub-`Border` elements with semi-transparent white backgrounds.

- [ ] **Step 3: Build and verify**

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/MinutesViewModel.cs MinuteMind/Views/MinutesPage.xaml
git commit -m "feat: build Minutes screen with bento card layout"
```

---

### Task 16: Build Edit Minutes Screen

**Files:**
- Modify: `MinuteMind/ViewModels/EditMinutesViewModel.cs`
- Modify: `MinuteMind/Views/EditMinutesPage.xaml`
- Create: `MinuteMind/Resources/Raw/editor.html`

- [ ] **Step 1: Create the rich text editor HTML template**

Create `MinuteMind/Resources/Raw/editor.html` — a self-contained HTML page with:
- `contenteditable="true"` div
- Styled with the app's font/color tokens (Plus Jakarta Sans for headings, Inter for body)
- Background matching Surface (#FAF9F8)
- JS functions: `getContent()` returns innerHTML, `setContent(html)` sets innerHTML, `execFormat(command)` wraps `document.execCommand`

- [ ] **Step 2: Implement EditMinutesViewModel**

Receives `MeetingId`, loads meeting, generates HTML from `MinutesJson` (or loads `HtmlContent` if previously edited). Commands for save (extract HTML via JS), close, and formatting.

- [ ] **Step 3: Build EditMinutesPage.xaml**

Layout matching `MinuteMind/mockups/edit_screen/code.html`:
- Modal presentation (close X + "Edit Minutes" + Save button in header)
- Borderless title Entry
- AI-REFINED badge + date
- WebView filling remaining space
- Fixed formatting toolbar at bottom (B/I/U/lists/quote/link + AI Polish)

- [ ] **Step 4: Build and verify**

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/ViewModels/EditMinutesViewModel.cs MinuteMind/Views/EditMinutesPage.xaml MinuteMind/Resources/Raw/editor.html
git commit -m "feat: build Edit Minutes screen with WebView rich text editor"
```

---

### Task 17: Build Export Screen

**Files:**
- Modify: `MinuteMind/ViewModels/ExportViewModel.cs`
- Modify: `MinuteMind/Views/ExportPage.xaml`

- [ ] **Step 1: Implement ExportViewModel**

Receives `MeetingId`, loads meeting. `DownloadPdfCommand` calls `IPdfExportService`, `ShareCommand` uses `Share.RequestAsync()`.

- [ ] **Step 2: Build ExportPage.xaml**

Layout matching `MinuteMind/mockups/export_screen/code.html`:
- Success state header (checkmark + "Your minutes are ready to share.")
- Document preview card with skeleton bars
- Full-width "Download PDF" gradient button
- Share with Team icons (Slack, Email, Drive, More)

- [ ] **Step 3: Build and verify**

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/ExportViewModel.cs MinuteMind/Views/ExportPage.xaml
git commit -m "feat: build Export screen with PDF download and share options"
```

---

### Task 18: Build Meetings (Search) Screen

**Files:**
- Modify: `MinuteMind/ViewModels/MeetingsViewModel.cs`
- Modify: `MinuteMind/Views/MeetingsPage.xaml`

- [ ] **Step 1: Implement MeetingsViewModel**

Full list from SQLite with search filtering and status filter chips. `FilteredMeetings` ObservableCollection recomputed on `SearchText` or `SelectedFilter` change.

- [ ] **Step 2: Build MeetingsPage.xaml**

- SearchBar
- Filter chips (All / Transcribed / Minutes Ready)
- CollectionView with same meeting card template as Dashboard
- Empty state "Quiet." label
- SwipeView for delete

- [ ] **Step 3: Build and verify**

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/MeetingsViewModel.cs MinuteMind/Views/MeetingsPage.xaml
git commit -m "feat: build Meetings screen with search and status filtering"
```

---

### Task 19: Build Settings Screen

**Files:**
- Modify: `MinuteMind/ViewModels/SettingsViewModel.cs`
- Modify: `MinuteMind/Views/SettingsPage.xaml`

- [ ] **Step 1: Implement SettingsViewModel**

Audio quality picker (persisted to `Preferences`), cache size display, clear cache command, app version.

- [ ] **Step 2: Build SettingsPage.xaml**

Grouped sections matching design system. Minimal — functional settings with "Coming Soon" placeholders for future features.

- [ ] **Step 3: Build and verify**

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/ViewModels/SettingsViewModel.cs MinuteMind/Views/SettingsPage.xaml
git commit -m "feat: build Settings screen with audio quality and storage options"
```

---

## Phase 4: Polish & Integration

### Task 20: Wire Up Real Audio Recording (Plugin.Maui.Audio)

**Files:**
- Modify: `MinuteMind/MinuteMind.csproj` (add NuGet)
- Modify: `MinuteMind/Services/Implementations/AudioRecorderService.cs`
- Modify: `MinuteMind/Platforms/Android/AndroidManifest.xml` (RECORD_AUDIO permission)
- Modify: `MinuteMind/Platforms/iOS/Info.plist` (NSMicrophoneUsageDescription)

- [ ] **Step 1: Add Plugin.Maui.Audio NuGet package**

```bash
cd MinuteMind && dotnet add package Plugin.Maui.Audio
```

- [ ] **Step 2: Add platform permissions**

Android `AndroidManifest.xml` — add:
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
```

iOS `Info.plist` — add:
```xml
<key>NSMicrophoneUsageDescription</key>
<string>MinuteMind needs microphone access to record meetings.</string>
```

- [ ] **Step 3: Implement real AudioRecorderService**

Replace the stub with real Plugin.Maui.Audio recording. Handle permission requests via `Permissions.RequestAsync<Permissions.Microphone>()`.

- [ ] **Step 4: Test on device**

Record audio, verify file is saved, verify it flows through to Processing screen.

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/MinuteMind.csproj MinuteMind/Services/Implementations/AudioRecorderService.cs MinuteMind/Platforms/
git commit -m "feat: wire up real audio recording with Plugin.Maui.Audio"
```

---

### Task 21: Implement Syncfusion PDF Export

**Files:**
- Modify: `MinuteMind/Services/Implementations/PdfExportService.cs`
- Modify: `MinuteMind/MauiProgram.cs` (register Syncfusion)

- [ ] **Step 1: Initialize Syncfusion in MauiProgram.cs**

Add `.ConfigureSyncfusionToolkit()` to the builder chain (check current Syncfusion.Maui.Toolkit API for exact method).

- [ ] **Step 2: Implement real PdfExportService**

Use Syncfusion PDF library to create a formatted PDF document from `MeetingMinutes` data — title, attendees, discussion points, decisions, action items. Style it to look professional.

- [ ] **Step 3: Test export flow**

Verify PDF is generated and can be opened/shared.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/Services/Implementations/PdfExportService.cs MinuteMind/MauiProgram.cs
git commit -m "feat: implement Syncfusion PDF export for meeting minutes"
```

---

### Task 22: Setup Local Transcription (Whisper.net)

**Files:**
- Modify: `MinuteMind/MinuteMind.csproj` (add Whisper.net NuGet)
- Modify: `MinuteMind/Services/Implementations/LocalTranscriptionService.cs`

- [ ] **Step 1: Add Whisper.net NuGet package**

```bash
cd MinuteMind && dotnet add package Whisper.net
dotnet add package Whisper.net.Runtime.Cpu
```

- [ ] **Step 2: Place Whisper model in Resources/Raw/**

Download a Whisper GGML model (e.g., `ggml-base.bin` ~142MB) and place in `MinuteMind/Resources/Raw/`. Note: this will significantly increase app size.

- [ ] **Step 3: Implement real LocalTranscriptionService**

Replace mock with real Whisper.net inference:
- Copy model from bundled assets to app data on first run
- Create `WhisperProcessor` with the model
- Process audio file, extract segments with timestamps
- Report progress via `IProgress<string>`
- Map Whisper output to `List<TranscriptSegment>`

- [ ] **Step 4: Test transcription with a real audio file**

- [ ] **Step 5: Commit**

```bash
git add MinuteMind/MinuteMind.csproj MinuteMind/Services/Implementations/LocalTranscriptionService.cs
git commit -m "feat: implement local on-device transcription with Whisper.net"
```

---

### Task 23: Remove Unused Packages + Final Cleanup

**Files:**
- Modify: `MinuteMind/MinuteMind.csproj`

- [ ] **Step 1: Remove System.IdentityModel.Tokens.Jwt**

```bash
cd MinuteMind && dotnet remove package System.IdentityModel.Tokens.Jwt
```

- [ ] **Step 2: Clean up any remaining TODO comments**

Search for `// TODO` across all files and resolve or document them.

- [ ] **Step 3: Final build and smoke test**

```bash
dotnet build MinuteMind/MinuteMind.csproj -t:Build -f net10.0-android
```

Deploy and test the full flow: Dashboard → Record → Processing → Transcript → Minutes → Edit → Export.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore: remove unused JWT package, resolve remaining TODOs"
```

# MinuteMind — Design Specification

## 1. Overview

MinuteMind is a .NET MAUI mobile app (Android + iOS) that lets users record or upload meeting audio, view AI-generated transcripts and structured meeting minutes, edit them, and export as PDF.

**Project goal**: Polished, production-grade UI with real audio recording and local SQLite storage. AI transcription and minutes generation are stubbed behind interfaces for future integration.

**Approach**: Pure MAUI controls + custom XAML styles (Approach A). WebView used only for the rich text editor on the Edit Minutes screen. CommunityToolkit.Mvvm for MVVM, sqlite-net-pcl for persistence, Syncfusion for PDF export, Plugin.Maui.Audio for recording.

**Design reference**: Stitch mockups in `MinuteMind/mockups/` with full HTML code and a design system doc at `MinuteMind/mockups/fluent_intelligence/DESIGN.md`.

---

## 2. Design System

### 2.1 Color Palette (Material Design 3 Tonal)

All values sourced from the Stitch mockup Tailwind config. Colors.xaml will be completely rewritten.

| Token | Hex | Usage |
|-------|-----|-------|
| Primary | `#005FAA` | High-intent actions, active tab, links |
| PrimaryContainer | `#0078D4` | Gradient CTA endpoints, tab active bg |
| OnPrimary | `#FFFFFF` | Text on primary backgrounds |
| OnPrimaryContainer | `#FFFFFF` | Text on primary container |
| Secondary | `#456084` | Secondary actions |
| SecondaryContainer | `#B8D3FD` | Status badges, AI chips |
| OnSecondaryContainer | `#405B7F` | Text on secondary container |
| Tertiary | `#974700` | Accent warnings |
| TertiaryContainer | `#BC5B00` | Tertiary filled elements |
| Error | `#BA1A1A` | Recording pulse, stop button, destructive actions |
| ErrorContainer | `#FFDAD6` | Error backgrounds |
| OnErrorContainer | `#93000A` | Text on error container |
| Surface | `#FAF9F8` | Base page background |
| SurfaceBright | `#FAF9F8` | Peak highlight |
| SurfaceContainer | `#EFEEED` | Card icon containers, section bg |
| SurfaceContainerLow | `#F4F3F2` | Subtle section boundaries |
| SurfaceContainerHigh | `#E9E8E7` | Secondary button bg |
| SurfaceContainerHighest | `#E3E2E1` | Avatar fallback bg |
| SurfaceContainerLowest | `#FFFFFF` | Elevated cards |
| SurfaceDim | `#DADAD9` | Disabled states |
| SurfaceTint | `#0060AB` | AI pulse glow |
| OnSurface | `#1A1C1C` | Primary text (never pure black) |
| OnSurfaceVariant | `#404752` | Secondary text, descriptions |
| Outline | `#717783` | Input borders |
| OutlineVariant | `#C0C7D4` | Ghost borders at low opacity |
| InverseSurface | `#2F3130` | Toast/tooltip bg |
| InverseOnSurface | `#F1F0EF` | Toast/tooltip text |
| InversePrimary | `#A3C9FF` | Inverse primary accent |
| PrimaryFixedDim | `#A3C9FF` | Decision card category labels |
| SecondaryFixedDim | `#ADC8F2` | Muted secondary highlights |
| PrimaryFixed | `#D3E3FF` | Light primary tints |
| TertiaryFixed | `#FFDBC8` | Attendee initial bg (tertiary) |
| OnPrimaryFixed | `#001C39` | Text on primary fixed |
| OnTertiaryFixed | `#311300` | Text on tertiary fixed |

### 2.2 Typography

Dual-font system:

| Role | Font Family | Weights | Usage |
|------|------------|---------|-------|
| Headline | Plus Jakarta Sans | ExtraBold (800), Bold (700), SemiBold (600) | Titles, headings, buttons, hero text |
| Body | Inter | Regular (400), Medium (500), SemiBold (600) | Body text, labels, timestamps, metadata |

Type scale (matching mockups):

| Style | Font | Size | Weight | Letter Spacing | Usage |
|-------|------|------|--------|----------------|-------|
| DisplayLarge | PlusJakarta | 34sp | ExtraBold | -0.02em | Hero "Capture Clarity." |
| HeadlineLarge | PlusJakarta | 28-36sp | ExtraBold | -0.02em (tight) | Page titles, meeting names |
| HeadlineMedium | PlusJakarta | 24sp | Bold | tight | Section headers |
| HeadlineSmall | PlusJakarta | 18-20sp | Bold | tight | Card titles, button text |
| TitleMedium | PlusJakarta | 16sp | SemiBold | tight | Meeting card titles |
| BodyLarge | Inter | 16sp | Regular | normal | Transcript text, descriptions |
| BodyMedium | Inter | 14-15sp | Regular/Medium | relaxed | Card descriptions, metadata |
| BodySmall | Inter | 13sp | Regular | normal | Timestamps, secondary info |
| LabelSmall | Inter | 10-11sp | Bold/SemiBold | +0.05em (widest), uppercase | Badges, chips, AI tags |
| LabelMedium | Inter | 12sp | Medium | wide | Tab bar labels |
| TimerDisplay | PlusJakarta | 64sp | ExtraBold | -0.04em (tighter) | Recording timer |

### 2.3 Elevation & Shadows

No hard borders. Depth via tonal layering + atmospheric shadows.

| Level | Technique | Usage |
|-------|-----------|-------|
| Base | Surface (#FAF9F8) | Page background |
| Section | SurfaceContainerLow (#F4F3F2) | Action items section bg |
| Card | SurfaceContainerLowest (#FFFFFF) on SurfaceContainer | Meeting cards, transcript segments |
| Ghost Shadow | `0px 12px 32px rgba(26,28,28,0.04)` | All elevated cards |
| CTA Shadow | `0px 8px 24px rgba(0,95,170,0.2)` | Primary gradient buttons |
| Nav Shadow | `0px -12px 32px rgba(26,28,28,0.04)` | Bottom tab bar (upward) |

Ghost border fallback: `OutlineVariant` at 10-15% opacity when extra definition is needed.

### 2.4 Corner Radii

| Element | Radius |
|---------|--------|
| Hero/Action cards | 32dp |
| Meeting cards | 24dp |
| Standard cards | 12dp |
| Buttons/pills | 9999dp (full round) |
| Tab bar top | 32dp |
| Speaker indicator | 8dp (lg) |
| Icon containers | 16dp |

### 2.5 Icons

Material Symbols Outlined. In MAUI, we'll use font icon approach or SVG images. Key icons per screen:

- Home: `menu`, `mic_none`, `upload_file`, `description`, `forum`, `psychology`, `chevron_right`, `auto_awesome`
- Recording: `auto_awesome`, `mic`, `pause_circle`, `stop_circle`
- Processing: `check`, `graphic_eq`, `auto_awesome`, `mic_none`
- Transcript: `arrow_back`, `search`, `more_horiz`, `auto_awesome`, `summarize`
- Minutes: `group`, `analytics`, `gavel`, `fact_check`, `download`, `auto_awesome`
- Edit: `close`, `format_bold/italic/underlined`, `format_list_bulleted/numbered`, `format_quote`, `link`, `magic_button`, `check_circle`, `radio_button_unchecked`
- Export: `close`, `check_circle`, `picture_as_pdf`, `download`, `forum`, `mail`, `add_to_drive`, `more_horiz`

---

## 3. Navigation

### 3.1 Shell TabBar (4 tabs)

| Tab | Label | Icon | Root Page | Active Style |
|-----|-------|------|-----------|-------------|
| Meetings | MEETINGS | `home_storage` | Dashboard | PrimaryContainer/10 bg pill, filled icon |
| Record | RECORD | `mic_none` | RecordingPage | Same |
| Search | SEARCH | `search` | MeetingsPage | Same |
| Settings | SETTINGS | `settings` | SettingsPage | Same |

Active tab: icon filled variant + `PrimaryContainer` text + `PrimaryContainer/10` background pill.
Inactive tab: `OnSurface` at 40% opacity.
Tab bar: white/80 background with backdrop blur effect (simulated via semi-transparent bg), upward ghost shadow, rounded top corners (32dp).
Label style: Inter 11sp medium, uppercase, wide tracking.

### 3.2 Route Registration

All routes registered in `AppShell.xaml.cs` via `Routing.RegisterRoute`:

| Route | Page | Presented As |
|-------|------|-------------|
| `Dashboard` | Dashboard | Tab root |
| `RecordingPage` | RecordingPage | Tab root (or pushed) |
| `MeetingsPage` | MeetingsPage | Tab root |
| `SettingsPage` | SettingsPage | Tab root |
| `ProcessingPage` | ProcessingPage | Pushed (hides tabs) |
| `TranscriptPage` | TranscriptPage | Pushed (hides tabs) |
| `MinutesPage` | MinutesPage | Pushed |
| `EditMinutesPage` | EditMinutesPage | Modal (close X) |
| `ExportPage` | ExportPage | Modal (close X) |

### 3.3 Tab Bar Visibility

- **Visible**: Dashboard, MinutesPage, MeetingsPage, ExportPage, SettingsPage
- **Hidden**: RecordingPage (when actively recording), ProcessingPage, TranscriptPage
- Edit screen presented as modal (no tab bar by nature)

---

## 4. Screen Specifications

### 4.1 Dashboard (Home Tab)

**Layout**: ScrollView → VerticalStackLayout

**Components**:
- Header: Grid — hamburger icon (left), "MinuteMind" label (PlusJakarta ExtraBold 24sp, Primary color), avatar Border (40dp circle, clipped image)
- Hero section: "Capture Clarity." (DisplayLarge), subtitle (BodyMedium, OnSurfaceVariant)
- Action cards: 2-column Grid, equal aspect ratio
  - Record Meeting: Border with linear gradient bg (Primary→PrimaryContainer), rounded 32dp, mic icon in frosted circle (white/20 bg, 48dp), title (HeadlineSmall white), subtitle (white/70)
  - Upload Audio: Border white bg, ghost shadow, rounded 32dp, upload icon in SecondaryContainer/30 circle, title (HeadlineSmall), subtitle (OnSurfaceVariant)
- "Recent Meetings" header: HeadlineMedium left + "VIEW ALL" (LabelSmall, Primary, uppercase tracking-widest) right
- Meeting list: CollectionView, each item is a Border (white, rounded 24dp, ghost shadow) containing:
  - Icon container (56dp, SurfaceContainer bg, rounded 16dp) with Material icon
  - Title (TitleMedium), date (BodySmall, OnSurfaceVariant), status badge (LabelSmall pill)
  - Chevron right icon
- AI Pulse: decorative auto_awesome icon with blur glow + "INTELLIGENT ANALYSIS ACTIVE" (LabelSmall, 40% opacity)

**ViewModel**: `DashboardViewModel`
- `RecentMeetings`: ObservableCollection<Meeting> (last 5, sorted by date desc)
- `RecordCommand`: navigates to RecordingPage
- `UploadCommand`: opens FilePicker (MP3/WAV/M4A), on selection navigates to ProcessingPage with audio path
- `ViewMeetingCommand(Meeting)`: navigates to MinutesPage with meeting ID
- `ViewAllCommand`: switches to Search tab

### 4.2 Recording Page

**Layout**: Full-screen Grid, Shell.TabBarIsVisible="False"

**Components**:
- Header: same app bar pattern
- "AI TRANSCRIPTION ACTIVE" chip: Border (SecondaryContainer/30 bg, pill shape), auto_awesome icon + LabelSmall text
- Meeting title: Label (HeadlineLarge, 28sp bold, centered)
- Metadata: "Started at {time} - {n} Participants Detected" (BodySmall, OnSurfaceVariant)
- Waveform: Custom `WaveformView` (extends GraphicsView) — 13 vertical bars with varying heights, Primary color at varying opacities
- Timer: Label (PlusJakarta 64sp ExtraBold, tabular-nums, OnSurface)
- Stop button: 96dp Border circle (white bg, ghost shadow) containing 32dp rounded-rectangle (Error color). Outer pulsing ring animation.
- Action buttons: HorizontalStackLayout
  - Pause: SurfaceContainerHigh bg pill, pause_circle icon + "Pause" text
  - Stop Recording: Primary gradient pill, stop_circle icon + "Stop Recording" text, CTA shadow
- Toast (fixed bottom): InverseSurface bg pill with mic icon + "Capturing from Internal Mic"
- Live Insight: Border card (white/80 bg, backdrop blur simulated, rounded 12dp) with "LIVE INSIGHT" label (LabelSmall Primary) + red pulse dot + transcript snippet (BodyMedium)

**ViewModel**: `RecordingViewModel`
- `IsRecording`, `IsPaused`: bool
- `ElapsedTime`: TimeSpan (formatted as HH:mm:ss via converter)
- `WaveformLevels`: float[] updated by timer for animation
- `LiveInsightText`: string (mock latest snippet)
- `MeetingTitle`: string (editable)
- `StartCommand`, `PauseCommand`, `StopCommand`
- Injects `IAudioRecorderService`

**Animations**:
- Waveform bars: DispatcherTimer randomizes heights every 150ms
- Stop button pulse: repeating ScaleTo animation on outer ring Border
- Red dot: FadeTo opacity pulse loop

### 4.3 Processing Page

**Layout**: Full-screen centered, Shell.TabBarIsVisible="False", back navigation disabled

**Components**:
- "Processing Meeting..." (HeadlineLarge, 28sp bold, centered)
- Central spinner: 192dp container with:
  - Outer ring: Border circle (SurfaceContainerHigh stroke, 6dp)
  - Animated arc: RotateTo animation on a partial-arc overlay (Primary, 6dp)
  - Inner: mic_none icon (Primary, 48sp) + "ANALYZING" (LabelSmall, Primary, uppercase tracking-widest)
- Processing steps (VerticalStackLayout, 16dp gap):
  - Completed step: white Border (rounded 12dp, ghost shadow), check icon in Primary/10 circle, bold title + size subtitle
  - Active step: white Border with 4dp Primary left border, slightly scaled (1.02x), spinner icon, bold Primary title, italic subtitle
  - Pending step: SurfaceContainerLow/50 bg, 60% opacity, muted icon, OnSurfaceVariant text
- Footer: "POWERED BY MINUTEMIND INTELLIGENCE V4.2" (LabelSmall, OnSurfaceVariant/40)

**ViewModel**: `ProcessingViewModel`
- `Steps`: ObservableCollection<ProcessingStep> (Title, Subtitle, Status enum: Completed/Active/Pending)
- `CurrentStepIndex`: int
- Auto-advances: Step 1 (2s) → Step 2 (3s) → Step 3 (2s) → auto-navigate to TranscriptPage
- Receives `audioFilePath` as navigation parameter
- Calls `ITranscriptionService.TranscribeAsync()` then `IMinutesGeneratorService.GenerateAsync()` (both mocked)
- Saves meeting to SQLite via `IMeetingRepository`

### 4.4 Transcript Page

**Layout**: ScrollView with CollectionView, tab bar hidden

**Components**:
- Header: back arrow + "Transcript" (PlusJakarta Bold 24sp, Primary) + search icon + avatar
- Meeting header section:
  - "LIVE SESSION" (LabelSmall, Primary, uppercase)
  - Title (HeadlineLarge, 36sp ExtraBold, tight tracking)
  - Overlapping speaker avatars (-8dp margin) + metadata (BodySmall)
- Transcript segments (CollectionView with DataTemplateSelector):
  - **SpeakerSegment template**: Border (white, rounded 12dp, ghost shadow)
    - Timestamp badge (LabelSmall, Primary bg/5, pill)
    - More menu icon (top right)
    - Speaker indicator: 40dp rounded-lg with colored bg + "S1"/"S2" (HeadlineSmall bold)
    - Speaker name (HeadlineSmall 14sp bold) + text (BodyLarge, OnSurfaceVariant, relaxed leading)
  - **AiInsightBreak template**: centered chip between segments
    - Horizontal rule (OutlineVariant/10)
    - "AI Identified Key Topic: {topic}" chip (SecondaryContainer/30 bg, pill, auto_awesome icon)
- Floating "Generate Minutes" FAB: gradient pill (Primary→PrimaryContainer), summarize icon + text, strong CTA shadow, fixed bottom center

**ViewModel**: `TranscriptViewModel`
- `Meeting`: Meeting model
- `TranscriptItems`: ObservableCollection (mixed list of SpeakerSegment and AiInsightBreak objects)
- `GenerateMinutesCommand`: navigates to MinutesPage (minutes already generated in Processing step)

### 4.5 Minutes Page

**Layout**: ScrollView → VerticalStackLayout, tab bar visible

**Components**:
- Header: hamburger + "Meeting Minutes" (PlusJakarta Bold 24sp) + avatar
- Hero section: 4dp Primary left border
  - Meeting type label (LabelSmall, OnSurfaceVariant/70, uppercase tracking-widest)
  - Title (HeadlineLarge, 36-40sp ExtraBold, tight)
  - Date + duration with calendar/clock icons (BodyMedium, OnSurfaceVariant)
  - Two buttons: "Edit Minutes" (outline pill, Primary text) + "Export to PDF" (gradient pill, white text, download icon)
- **Attendees card**: white Border (rounded 12dp, ghost shadow), group icon + "Attendees" heading, list of names with colored initial circles (32dp, various container colors)
- **Key Discussion Points card**: white Border, analytics icon, bullet list with Primary-colored 6dp dots
- **Decisions card**: FULL-WIDTH Primary background Border (rounded 12dp, primary shadow), white text
  - gavel icon + "Decisions" heading
  - 2-column Grid of frosted sub-cards (white/10 bg, white/10 border, rounded 8dp)
  - Each: category label (PrimaryFixedDim, LabelSmall uppercase) + decision text (18sp bold italic white)
- **Action Items section**: SurfaceContainerLow bg Border (rounded 12dp, OutlineVariant/10 border)
  - fact_check icon + "Action Items" heading
  - 2-column Grid of checkbox items: each is a Border (white bg, rounded 8dp) with CheckBox + title (TitleMedium) + assignee/due (BodySmall, OnSurfaceVariant)
  - Completed items: line-through + 50% opacity
- "AI-Generated Intelligence Layer" badge: centered, SecondaryContainer/30 bg pill

**ViewModel**: `MinutesViewModel`
- `Meeting`: Meeting model (title, type, date, duration)
- `Attendees`: ObservableCollection<string>
- `DiscussionPoints`: ObservableCollection<string>
- `Decisions`: ObservableCollection<Decision> (Category, Text)
- `ActionItems`: ObservableCollection<ActionItem>
- `ToggleActionItemCommand(ActionItem)`: toggles IsCompleted, saves to SQLite
- `EditCommand`: navigates to EditMinutesPage (modal)
- `ExportCommand`: navigates to ExportPage (modal)

### 4.6 Edit Minutes Page

**Layout**: Full-screen, presented as modal (close X)

**Components**:
- Header: X close button + "Edit Minutes" (PlusJakarta Bold 24sp, Primary) + "Save Changes" pill button (PrimaryContainer bg)
- Title input: Entry styled borderless/transparent (PlusJakarta 30sp ExtraBold), placeholder "Untitled Meeting"
- "AI-REFINED" badge (SecondaryContainer bg, LabelSmall) + date (BodySmall)
- Rich text area: WebView loading an HTML template from `Resources/Raw/editor.html` with `contenteditable="true"` div
  - Pre-populated with meeting minutes formatted as HTML (summary paragraph, Key Decisions heading + bullet list, Action Items with check icons)
  - Styled with the app's font/color tokens via inline CSS in the template
  - Communication: ViewModel → WebView via `EvaluateJavaScriptAsync`, WebView → ViewModel via `Navigating` event or JS `window.location` hack for content extraction
- Fixed bottom toolbar: Grid of icon buttons
  - Format: Bold, Italic, Underline | Bullet list, Numbered list | Quote, Link
  - Each: 40dp tap target, OnSurfaceVariant icon, SurfaceContainer hover bg
  - Dividers: 1dp vertical lines (OutlineVariant/30)
  - "AI Polish" pill button: SurfaceContainerLowest bg, OutlineVariant/20 border, magic_button icon + label

**ViewModel**: `EditMinutesViewModel`
- `Title`: string (two-way bound)
- `HtmlContent`: string (loaded into WebView, extracted on save)
- `HasChanges`: bool
- `SaveCommand`: extracts HTML via JS interop, updates meeting in SQLite, closes modal
- `CloseCommand`: if HasChanges prompt discard, else close
- Format commands (`FormatBoldCommand`, etc.): invoke `document.execCommand()` via `EvaluateJavaScriptAsync`
- `AiPolishCommand`: placeholder (future AI rewrite)

### 4.7 Export Page

**Layout**: ScrollView, presented as modal (close X), tab bar visible when reached from Minutes

**Components**:
- Header: X close + "Export Minutes" + avatar
- Success state: checkmark circle (48dp, SecondaryContainer bg, filled check_circle Primary icon) + "Your minutes are ready to share." (HeadlineLarge 28sp ExtraBold) + subtitle (BodyMedium, OnSurfaceVariant)
- Document preview card: Border (white, rounded 32dp, ghost shadow, OutlineVariant/10 border)
  - "DOCUMENT PREVIEW" label (LabelSmall, Primary) + filename (HeadlineMedium bold) + date+size (BodySmall)
  - PDF icon in SurfaceContainerLow square (top right)
  - Visual placeholder: SurfaceContainerLow area with skeleton bars (SurfaceContainerHighest rounded-full at varying widths/opacities) mimicking document content
- "Download PDF" button: full-width 64dp gradient pill (Primary→PrimaryContainer), download icon + text (HeadlineSmall bold white), CTA shadow
- "SHARE WITH TEAM" label (LabelSmall, OnSurfaceVariant, centered, uppercase tracking)
- 4 share buttons in equal-width HorizontalStackLayout:
  - Each: 56dp rounded-2xl Border (white bg, OutlineVariant/10 border, subtle shadow) + icon + label (LabelSmall uppercase)
  - Slack (forum icon, #4A154B), Email (mail icon, Primary), Drive (add_to_drive icon, #34A853), More (more_horiz icon, OnSurfaceVariant)

**ViewModel**: `ExportViewModel`
- `DocumentName`, `ExportDate`, `FileSize`: display properties
- `DownloadPdfCommand`: calls `IPdfExportService.ExportAsync()`, saves file, shows success
- `ShareCommand(string target)`: uses MAUI `Share.RequestAsync()` with the generated PDF file
- Receives meeting ID as navigation parameter

### 4.8 Meetings Page (Search Tab)

**Layout**: VerticalStackLayout with SearchBar + CollectionView

**Components**:
- SearchBar at top, styled to match design system (SurfaceContainerLow bg on inactive, white on focus with Primary bottom accent)
- Filter chips: HorizontalStackLayout of pill Borders — "All", "Transcribed", "Minutes Ready"
  - Active chip: Primary bg, white text
  - Inactive chip: SurfaceContainerHigh bg, OnSurface text
- Meeting list: CollectionView reusing same card DataTemplate as Dashboard
- Empty state: single "Quiet." label (DisplayLarge, centered, OnSurfaceVariant)
- SwipeView wrapping each item for swipe-to-delete

**ViewModel**: `MeetingsViewModel`
- `AllMeetings`: full list from SQLite
- `FilteredMeetings`: ObservableCollection (filtered by search + status filter)
- `SearchText`: string
- `SelectedFilter`: enum (All, Transcribed, MinutesReady)
- `ViewMeetingCommand(Meeting)`: navigates to MinutesPage
- `DeleteMeetingCommand(Meeting)`: confirms then deletes from SQLite

### 4.9 Settings Page

**Layout**: ScrollView → VerticalStackLayout of grouped sections

**Components**:
- Audio section: audio quality picker (Low/Medium/High)
- Storage section: cache size display + "Clear Cache" button
- About section: app version, "Powered by MinuteMind Intelligence"
- Future placeholders (disabled/labeled "Coming Soon"): AI Provider, Theme toggle, Account

**ViewModel**: `SettingsViewModel`
- `AudioQuality`: enum, persisted to Preferences
- `CacheSize`: string
- `ClearCacheCommand`
- `AppVersion`: string

---

## 5. Data Layer

### 5.1 Models

```csharp
public class Meeting
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public string MeetingType { get; set; }        // "Internal Sync", "Client Call", etc.
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public MeetingStatus Status { get; set; }       // Recording, Processing, Transcribed, MinutesReady
    public string AudioFilePath { get; set; }
    public string TranscriptJson { get; set; }      // Serialized List<TranscriptSegment>
    public string MinutesJson { get; set; }          // Serialized MeetingMinutes
    public string HtmlContent { get; set; }          // User-edited rich text (from Edit screen)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum MeetingStatus { Recording, Processing, Transcribed, MinutesReady }

public class TranscriptSegment
{
    public TimeSpan Timestamp { get; set; }
    public string Speaker { get; set; }
    public string Text { get; set; }
}

public class AiInsightBreak
{
    public string Topic { get; set; }
}

public class MeetingMinutes
{
    public List<string> Attendees { get; set; }
    public List<string> DiscussionPoints { get; set; }
    public List<Decision> Decisions { get; set; }
    public List<ActionItem> ActionItems { get; set; }
}

public class Decision
{
    public string Category { get; set; }
    public string Text { get; set; }
}

public class ActionItem
{
    public string Description { get; set; }
    public string Assignee { get; set; }
    public string DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
```

### 5.2 SQLite Database

Single `Meeting` table. `TranscriptJson` and `MinutesJson` are serialized/deserialized via `System.Text.Json`. The database file lives in `FileSystem.AppDataDirectory`.

```csharp
public class MinuteMindDatabase
{
    private SQLiteAsyncConnection _db;

    async Task Init()
    {
        if (_db != null) return;
        var path = Path.Combine(FileSystem.AppDataDirectory, "minutemind.db3");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<Meeting>();
    }
}
```

### 5.3 Service Interfaces

| Interface | Methods | Notes |
|-----------|---------|-------|
| `IAudioRecorderService` | `StartAsync()`, `PauseAsync()`, `ResumeAsync()`, `StopAsync() → string` | Returns audio file path. Uses Plugin.Maui.Audio. |
| `ITranscriptionService` | `TranscribeAsync(string audioPath) → List<TranscriptSegment>` | Mock: 3s delay, returns 4 sample segments + 1 AI insight break |
| `IMinutesGeneratorService` | `GenerateAsync(List<TranscriptSegment>) → MeetingMinutes` | Mock: 2s delay, returns sample structured minutes |
| `IMeetingRepository` | `GetAllAsync()`, `GetRecentAsync(int count)`, `GetByIdAsync(int)`, `SaveAsync(Meeting)`, `DeleteAsync(Meeting)` | SQLite CRUD via MinuteMindDatabase |
| `IPdfExportService` | `ExportAsync(Meeting) → string` | Returns PDF file path. Uses Syncfusion. |
| `INavigationService` | `GoToAsync(string)`, `GoToAsync(string, Dictionary)`, `GoBackAsync()`, `NavigateToShellAsync()` | Already exists, needs DI registration |

### 5.4 DI Registration (MauiProgram.cs)

```
// Database
Singleton: MinuteMindDatabase

// Services
Singleton: IMeetingRepository → MeetingRepository
Singleton: INavigationService → NavigationService
Singleton: IAudioRecorderService → AudioRecorderService
Transient: ITranscriptionService → MockTranscriptionService
Transient: IMinutesGeneratorService → MockMinutesGeneratorService
Transient: IPdfExportService → PdfExportService

// ViewModels (Transient)
DashboardViewModel, RecordingViewModel, ProcessingViewModel,
TranscriptViewModel, MinutesViewModel, EditMinutesViewModel,
ExportViewModel, MeetingsViewModel, SettingsViewModel

// Pages (Transient)
Dashboard, RecordingPage, ProcessingPage, TranscriptPage,
MinutesPage, EditMinutesPage, ExportPage, MeetingsPage, SettingsPage
```

---

## 6. Custom Controls

### 6.1 WaveformView

Extends `GraphicsView` with `IDrawable`. Draws 13 vertical rounded bars at varying heights. Heights updated via bindable `float[] Levels` property. A `DispatcherTimer` in the ViewModel randomizes levels every 150ms during recording.

Bar styling: 6dp wide, Primary color at opacities proportional to height (20%-100%), 3dp rounded caps, 6dp horizontal gap.

### 6.2 ProcessingStepView

Reusable ContentView for the 3-step processing cards. Bindable properties: `Title`, `Subtitle`, `StepStatus` (enum: Completed/Active/Pending). Visual state changes icon, border, text color, opacity, and scale based on status.

---

## 7. NuGet Packages

Already in .csproj (keep):
- `CommunityToolkit.Maui` 14.1.0 — animations, behaviors
- `CommunityToolkit.Mvvm` 8.4.2 — ObservableObject, RelayCommand, ObservableProperty
- `sqlite-net-pcl` 1.9.172 + `SQLitePCLRaw.bundle_green` 2.1.10 — local database
- `Syncfusion.Maui.Toolkit` 1.0.9 — PDF generation
- `Microsoft.Extensions.Http` 10.0.0 — future API calls

To add:
- `Plugin.Maui.Audio` — cross-platform audio recording (Android + iOS)

To remove (not needed for v1):
- `System.IdentityModel.Tokens.Jwt` 8.14.0 — no auth in this version

---

## 8. Platform Considerations

### Android
- Microphone permission (`RECORD_AUDIO`) — request at runtime before recording
- Storage permission for saving PDFs (scoped storage on Android 11+)
- Update `Platforms/Android/Resources/values/colors.xml` to match brand palette
- Min SDK 21 (already set)

### iOS
- `NSMicrophoneUsageDescription` in Info.plist
- `NSPhotoLibraryAddUsageDescription` if saving PDFs to Files
- Min iOS 15.0 (already set)

---

## 9. Out of Scope (v1)

- Real AI transcription/minutes generation (interfaces ready for future integration)
- User authentication / JWT
- Dark mode (design system supports it via AppThemeBinding but not a v1 priority)
- Cloud sync / remote API
- Real-time live transcription during recording (Live Insight shows mock snippets)
- Speaker diarization
- Slack/Email/Drive share integrations (use generic Share API, specific icons are decorative)

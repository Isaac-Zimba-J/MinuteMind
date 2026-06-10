# MinuteMind — Claude Code Context

> Keep this file up to date as the project evolves. It is the single source of truth for any AI-assisted session.  
> Last updated: 2026-06-09

---

## What This App Is

**MinuteMind** is a fully offline .NET 10 MAUI mobile app that:
1. Records meetings via the device microphone
2. Transcribes audio on-device using the bundled Whisper ML model (`ggml-tiny.bin`)
3. Generates structured meeting minutes (currently stubbed — real LLM integration pending)
4. Lets users edit and export minutes as PDF

**Primary target:** Android Play Store (first release)  
**Secondary targets:** iOS App Store, MacCatalyst, Windows  
**No cloud required** — all processing is on-device

---

## Architecture Rules

### MVVM — Non-Negotiable

- **Views are pure XAML.** No code-behind logic. Only `InitializeComponent()` is acceptable.
- **All state and logic lives in ViewModels** using `CommunityToolkit.Mvvm`.
- Use `[ObservableProperty]` for bindable properties — never manually implement `INotifyPropertyChanged`.
- Use `[RelayCommand]` for async commands — never manually implement `ICommand`.
- ViewModels are **stateless on entry**: data is received via `ApplyQueryAttributes`, not stored between sessions.
- Constructor parameters = DI-injected services only.

### Dependency Injection

| Lifetime | Used For |
|----------|----------|
| `Singleton` | Database, repositories, audio manager, navigation, long-lived services |
| `Transient` | ViewModels, Views, per-call services (PDF export, minutes generator) |

- Register everything in `MauiProgram.cs`.
- Never use `ServiceLocator` or `new ViewModel()` directly.

### Navigation

- All navigation via `INavigationService` — never call `Shell.Current` directly from a ViewModel.
- Route names = `nameof(Views.SomePage)` — no magic strings.
- Pass data via `Dictionary<string, object>` query parameters.
- Receiving page implements `IQueryAttributable.ApplyQueryAttributes()`.
- `ApplyQueryAttributes` triggers the relevant Load command — page is fully self-contained on entry.

### Services

- Every external/platform capability must have an interface in `Services/Contracts/`.
- Concrete implementation goes in `Services/Implementations/`.
- Unbuilt features use a `Mock*Service` stub returning dummy data so UI can be developed first.
- Interfaces must be unit-testable — no static calls or platform APIs inside implementations unless wrapped.

### Data Layer

- `MinuteMindDatabase` is the raw SQLite connection — only `MeetingRepository` touches it.
- `MeetingRepository` is the **only** path to database reads/writes in the app.
- Nested objects (transcript segments, minutes) are JSON-serialized into string columns in `Meetings`.
- New tables → update `MinuteMindDatabase.InitAsync()`.

---

## Folder Structure

```
MinuteMind/
├── Models/              # Domain models + enums (no business logic)
├── ViewModels/          # All UI state + commands
├── Views/               # XAML pages (no logic)
├── Services/
│   ├── Contracts/       # Interfaces for every platform/external capability
│   └── Implementations/ # Concrete implementations + Mock* stubs
├── Data/                # MinuteMindDatabase.cs (raw SQLite wrapper)
├── Controls/            # Reusable custom MAUI controls
├── Resources/
│   ├── Styles/          # Colors.xaml, Styles.xaml — single source of truth for tokens
│   ├── Fonts/           # Plus Jakarta Sans + Inter
│   ├── Images/          # PNG icons
│   └── Raw/             # ggml-tiny.bin (bundled Whisper model, ~75 MB)
├── Platforms/           # Per-platform entry points + manifests
├── MauiProgram.cs       # DI registration + app bootstrap
└── AppShell.xaml        # Shell navigation + route declarations
```

---

## Design System

### Colors

All tokens are in `Resources/Styles/Colors.xaml`. **Always use `StaticResource` — never hardcode hex.**

| Token | Value | Use |
|-------|-------|-----|
| `Primary` | `#005FAA` | Buttons, active icons, key accents |
| `PrimaryContainer` | `#0078D4` | Filled button backgrounds |
| `Surface` | `#FAF9F8` | Page backgrounds |
| `SurfaceContainer` | `#EEF0F0` | Card backgrounds |
| `SurfaceContainerLowest` | `#FFFFFF` | Elevated/modal surfaces |
| `OnSurface` | `#1A1C1C` | Primary text |
| `OnSurfaceVariant` | `#404752` | Secondary / muted text |
| `OutlineVariant` | *(see Colors.xaml)* | Borders, separators |
| `Error` | `#BA1A1A` | Error states |

### Typography

Never set `FontFamily` inline. Reference styles from `Styles.xaml`.

- Headlines → **Plus Jakarta Sans** (Bold / SemiBold / Medium)
- Body text → **Inter** (Regular / Medium / SemiBold)
- Key styles: `DisplayLarge` (34sp hero), `HeadlineLarge` (28sp), `HeadlineSmall` (18sp), `BodyLarge` (16sp), `BodyMedium` (14sp), `LabelSmall` (10sp uppercase badge), `TimerDisplay` (64sp recording timer)

### Spacing

- Page padding: **24px**
- Card padding: **16px**
- Component gap: **8px**
- Section gap: **16–24px**

### Shape

- Large cards / containers: **24px** corner radius
- Buttons: **20px** (pill-shaped)
- Chips / badges: **8px**
- Shadows: `Radius: 32, Opacity: 0.04, Offset: 0,12` (ghost elevation)

---

## App Navigation Map

```
TabBar
├── [Tab 1] Dashboard          → route: "Dashboard"
├── [Tab 2] RecordingPage      → route: "RecordingPage"
├── [Tab 3] MeetingsPage       → route: "MeetingsPage"
└── [Tab 4] SettingsPage       → route: "SettingsPage"

Modal / Overlay routes (no tab, registered in AppShell.xaml.cs):
├── ProcessingPage             → receives: AudioPath, MeetingTitle, Duration (ticks)
├── TranscriptPage             → receives: MeetingId
├── MinutesPage                → receives: MeetingId
├── EditMinutesPage            → receives: MeetingId
└── ExportPage                 → receives: MeetingId
```

---

## Full Feature Status

### Screens

| Screen | ViewModel | XAML | Status |
|--------|-----------|------|--------|
| Dashboard | DashboardViewModel | Dashboard.xaml | UI done, data hardcoded |
| Recording | RecordingViewModel | RecordingPage.xaml | Done (waveform uses mock levels) |
| Meetings list | MeetingsViewModel | MeetingsPage.xaml | UI done, search not wired |
| Settings | SettingsViewModel | SettingsPage.xaml | UI done, toggles don't persist |
| Processing | ProcessingViewModel | ProcessingPage.xaml | Done |
| Transcript | TranscriptViewModel | TranscriptPage.xaml | Done |
| Minutes | MinutesViewModel | MinutesPage.xaml | Done |
| Edit Minutes | EditMinutesViewModel | EditMinutesPage.xaml | Done |
| Export | ExportViewModel | ExportPage.xaml | Done |

### Services

| Service | Interface | Implementation | Status |
|---------|-----------|----------------|--------|
| Meeting storage | `IMeetingRepository` | `MeetingRepository` | Done |
| Audio recording | `IAudioRecorderService` | `AudioRecorderService` | Done |
| Local transcription | `ITranscriptionService` | `LocalTranscriptionService` | Done (Whisper.net, ggml-tiny) |
| Minutes generation | `IMinutesGeneratorService` | `MockMinutesGeneratorService` | **STUB — returns hardcoded data** |
| PDF export | `IPdfExportService` | `PdfExportService` | Done (QuestPDF Community) |
| Navigation | `INavigationService` | `NavigationService` | Done |

### Known Stubs / Gaps

| Area | Issue | File |
|------|-------|------|
| **Build** | "Program path is null or empty" — Android build broken | Run `dotnet build -f net10.0-android` to see real errors |
| **Minutes AI** | `MockMinutesGeneratorService` returns hardcoded fake minutes | `MockMinutesGeneratorService.cs` |
| **Dashboard data** | Recent meetings list is 3 hardcoded cards, not from SQLite | `DashboardViewModel.cs` |
| **Waveform levels** | Random values, not real microphone amplitude | `RecordingViewModel.cs` |
| **Settings** | Dark mode / language / auto-transcribe toggles don't persist | `SettingsViewModel.cs` |
| **Upload audio** | `UploadAudio` command exists but no file picker wired | `DashboardViewModel.cs` |
| **Meeting delete** | No delete UI or command anywhere | `MeetingsViewModel.cs` |
| **Permission denied** | No user-facing message when mic is denied | `AudioRecorderService.cs` |
| **PDF save location** | Saves to internal `AppDataDirectory`, not public Downloads | `PdfExportService.cs` |

---

## Key Technical Constraints

1. **Whisper model is ~75 MB bundled.** Increases AAB significantly. Future: download on first launch.
2. **Transcription is CPU-intensive** (30s–3min on Android). Needs Android foreground service to prevent kill during background.
3. **Audio recording format:** `Plugin.Maui.Audio` output format must be WAV for the `LocalTranscriptionService` WAV parser. Verify before testing on device.
4. **QuestPDF Community License:** Set `QuestPDF.Settings.License = LicenseType.Community` in `MauiProgram.cs` before any PDF generation.
5. **Android scoped storage (API 29+):** Saving to public Downloads requires `MediaStore` API, not `File.Create()`.
6. **Android target SDK 34** required for new Play Store submissions (2024+).

---

## Adding a New Feature (Checklist)

1. **Model** — add to `Models/` if new data is needed; update `MinuteMindDatabase.InitAsync()` if persisted
2. **Interface** — new platform/external capability = new interface in `Services/Contracts/`
3. **Mock** — implement `Mock*Service` stub so UI can be built before the real service is ready
4. **ViewModel** — extend or add, using `[ObservableProperty]` + `[RelayCommand]`
5. **View** — XAML only, bound to ViewModel, all colors/styles from `StaticResource`
6. **Register** — add service + ViewModel + View in `MauiProgram.cs`
7. **Route** — register in `AppShell.xaml.cs` if it's a new page: `Routing.RegisterRoute(nameof(Views.NewPage), typeof(Views.NewPage))`
8. **Replace stub** — swap `Mock*Service` for real implementation before shipping

---

## NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Maui.Controls` | 10.0.41 | Core framework |
| `CommunityToolkit.Maui` | 14.1.0 | MAUI helpers |
| `CommunityToolkit.Mvvm` | 8.4.2 | MVVM source generators |
| `Plugin.Maui.Audio` | 4.0.0 | Audio recording + playback |
| `Whisper.net` + `Whisper.net.Runtime` | 1.9.0 | On-device speech-to-text |
| `sqlite-net-pcl` + `SQLitePCLRaw.bundle_green` | 1.9.172 / 2.1.10 | Local database |
| `QuestPDF` | 2026.2.4 | PDF generation |
| `Microsoft.Extensions.Http` | 10.0.0 | HttpClientFactory (future API calls) |
| `DotNetMeteor.HotReload.Plugin` | 3.3.0 | Dev-time hot reload |

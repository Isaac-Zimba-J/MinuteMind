# MinuteMind — Play Store Readiness

> Track this file as features are completed. Update status column as work progresses.  
> Last updated: 2026-06-09

Legend: ❌ Not started | ⚠️ Partial / stubbed | ✅ Done

---

## 🔴 Critical — App Cannot Ship Without These

### Build & Stability

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Fix Android build ("Program path is null or empty") | ❌ | Run `dotnet build -f net10.0-android` to find real errors |
| 2 | Successful release build (`.aab`) | ❌ | Required for Play Store submission |
| 3 | App signing (generate release keystore, configure `MinuteMind.csproj`) | ❌ | `KeystorePath`, `KeystorePass`, `KeyAlias`, `KeyPass` in csproj |
| 4 | ProGuard / R8 rules for release build | ❌ | Whisper.net and QuestPDF need keep rules to avoid stripping |
| 5 | Target SDK 34 set in `AndroidManifest.xml` | ❌ | Required for all new Play Store submissions |

### Core Features (Currently Stubbed)

| # | Task | Status | Notes |
|---|------|--------|-------|
| 6 | Real AI minutes generation | ❌ | Replace `MockMinutesGeneratorService` — options: Claude API, on-device LLM, or cloud API |
| 7 | Dashboard shows real recent meetings from SQLite | ⚠️ | `DashboardViewModel` hardcodes 3 cards — wire to `IMeetingRepository.GetRecentAsync()` |
| 8 | Settings persistence (dark mode, language, auto-transcribe) | ⚠️ | Use `Preferences.Default.Set/Get` + apply `Application.Current.UserAppTheme` for dark mode |
| 9 | Upload audio from file picker | ⚠️ | `UploadAudio` command exists but not wired — use `FilePicker.PickAsync()` then navigate to ProcessingPage |

### Android UX — Will Cause 1-Star Reviews If Missing

| # | Task | Status | Notes |
|---|------|--------|-------|
| 10 | Microphone permission denied → show actionable message + deep link to settings | ❌ | `AudioRecorderService.StartAsync()` silently fails today |
| 11 | Android foreground service for transcription | ❌ | Without it, Android kills the process if user backgrounds the app during a long transcription |
| 12 | PDF save to public Downloads folder (MediaStore API) | ⚠️ | `PdfExportService` saves to `AppDataDirectory` — not accessible to user or file manager |
| 13 | Meeting deletion | ❌ | No delete command or swipe-to-delete in `MeetingsPage` / `MeetingsViewModel` |
| 14 | Empty state when no meetings exist | ❌ | `MeetingsPage` and `Dashboard` crash or show nothing on first launch |

### Store Requirements — Google Will Reject Without These

| # | Task | Status | Notes |
|---|------|--------|-------|
| 15 | Privacy Policy (hosted URL) | ❌ | Required — app records audio and stores meeting content locally |
| 16 | Data Safety form completed in Play Console | ❌ | Declare: audio not transmitted, no data shared, local storage only |
| 17 | `RECORD_AUDIO` permission justification | ❌ | Sensitive permission — must explain use case in Play Console declaration |
| 18 | Content rating questionnaire completed | ❌ | In Play Console — select appropriate rating |
| 19 | App icon 512×512 PNG (no alpha) | ❌ | Current icon is SVG — export to PNG at correct size |
| 20 | Feature graphic 1024×500 PNG | ❌ | Required for store listing page |
| 21 | At least 2 phone screenshots | ❌ | Show recording, transcript, and minutes pages |
| 22 | Short description (≤80 chars) | ❌ | Play Console listing field |
| 23 | Full description (≤4000 chars) | ❌ | Play Console listing field |

---

## 🟡 Important — Needed for Good Ratings

### Polish & Error Handling

| # | Task | Status | Notes |
|---|------|--------|-------|
| 24 | Real audio waveform levels | ⚠️ | `RecordingViewModel` uses `Random` — use `Plugin.Maui.Audio` amplitude API |
| 25 | Error state for transcription failure | ❌ | If Whisper throws, user sees a blank screen — show actionable error |
| 26 | Error state for PDF export failure | ❌ | `ExportViewModel` has no error handling |
| 27 | Loading states (shimmer/skeleton) on Meetings list | ❌ | List shows empty while loading from DB |
| 28 | Processing page handles app backgrounding | ❌ | Progress survives if user switches apps during long transcription |
| 29 | Handle very long recordings (>30 min) gracefully | ❌ | Chunked Whisper processing or user warning before starting |

### UX Improvements

| # | Task | Status | Notes |
|---|------|--------|-------|
| 30 | Search bar wired to real DB filter | ⚠️ | `MeetingsViewModel.SearchQuery` exists but `LoadMeetings` doesn't filter on it |
| 31 | Meeting rename from list | ❌ | Tap title in MeetingsPage → inline edit |
| 32 | Pull-to-refresh on Meetings list | ❌ | Standard mobile UX expectation |
| 33 | Action item completion toggle in MinutesPage | ❌ | `ActionItem.IsCompleted` exists but no UI toggle |
| 34 | Copy transcript segment to clipboard | ❌ | Per-segment tap/long-press copy button |
| 35 | Meeting deletion with undo (snackbar) | ❌ | After item #13 above — add undo snackbar |
| 36 | Sort meetings by date / duration / title | ❌ | Picker in MeetingsPage header |

### Settings — Make Toggles Actually Work

| # | Task | Status | Notes |
|---|------|--------|-------|
| 37 | Dark mode: persist + apply `AppTheme.Dark` | ⚠️ | Toggle exists, `UserAppTheme` not set |
| 38 | Language selection: pass to `ITranscriptionService` | ⚠️ | Whisper supports language hint — wire through `TranscribeAsync()` |
| 39 | Auto-transcribe: skip confirmation after stop | ⚠️ | If true, go directly from RecordingPage to ProcessingPage without dialog |

---

## 🟢 Nice to Have — Post-Launch (v1.1+)

### AI / Intelligence Upgrades

| # | Task | Status | Notes |
|---|------|--------|-------|
| 40 | Speaker diarization (distinguish multiple speakers) | ❌ | All segments currently tagged "Speaker 1" |
| 41 | Meeting type auto-classification (standup, 1:1, all-hands) | ❌ | Infer from transcript content |
| 42 | Confidence scores on action items | ❌ | Useful for review before sharing |
| 43 | Whisper model upgrade (base or small) | ❌ | Tiny is fast but has lower accuracy on non-English / accented speech |

### Performance

| # | Task | Status | Notes |
|---|------|--------|-------|
| 44 | Download Whisper model on first launch (not bundled) | ❌ | Reduces APK from ~100MB+ to ~5MB. Show progress during download. |
| 45 | Background transcription service | ❌ | Let app run transcription in background without keeping screen on |
| 46 | Streaming transcript display (show words as they're transcribed) | ❌ | Whisper.net supports streaming segments — pipe to TranscriptPage live |

### Platform Expansion

| # | Task | Status | Notes |
|---|------|--------|-------|
| 47 | iOS App Store release | ❌ | Separate signing + App Store Connect setup |
| 48 | iCloud / Google Drive sync | ❌ | Backup meetings across devices |
| 49 | Home screen widget (quick-record button) | ❌ | Android App Widget |
| 50 | Siri Shortcuts / Google Assistant integration | ❌ | "Start a meeting in MinuteMind" voice command |

### Export & Sharing

| # | Task | Status | Notes |
|---|------|--------|-------|
| 51 | Email minutes directly from app | ❌ | Use `MailTo` or `Share.Default` |
| 52 | Export to Notion / Confluence | ❌ | API integration |
| 53 | Export to Google Docs | ❌ | Google Drive API |
| 54 | Shareable read-only link (minutes as web page) | ❌ | Requires backend |

---

## Priority Order for First Release

Work these in order:

1. **Fix build** (#1) — nothing else matters until the app compiles
2. **Real minutes generation** (#6) — the core value prop is fake right now
3. **Dashboard real data** (#7) + **Settings persist** (#8) — basic product hygiene
4. **Empty states + error handling** (#14, #25, #26) — prevents blank screens
5. **Foreground service** (#11) — prevents Android killing transcription
6. **Permission denied UX** (#10) — common first-run failure
7. **PDF to Downloads** (#12) — users expect to find their file
8. **Meeting deletion** (#13) — without this users are stuck with old recordings
9. **App signing + release build** (#2, #3, #4) — last engineering step before submission
10. **Store listing assets + legal** (#15–#23) — submit

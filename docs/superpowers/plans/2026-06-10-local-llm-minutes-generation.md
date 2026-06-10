# Local LLM Minutes Generation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace `MockMinutesGeneratorService` with `LlmMinutesGeneratorService` that downloads Qwen2.5 0.5B on first launch and runs fully on-device to generate structured meeting minutes from a Whisper transcript.

**Architecture:** `ProcessingViewModel` calls `IMinutesGeneratorService.GenerateAsync(transcript, progress)`. On first call, `LlmMinutesGeneratorService` streams the GGUF model (~300 MB) from Hugging Face to `AppDataDirectory`, then loads it via LLamaSharp and runs a single-turn ChatML inference. The resulting JSON is deserialized into `MeetingMinutes` with a safe fallback on parse failure. The model is disposed after each call to avoid holding 300 MB in RAM between meetings.

**Tech Stack:** LLamaSharp 0.20.0, LLamaSharp.Backend.Cpu 0.20.0, Qwen2.5-0.5B-Instruct-Q4_K_M.gguf, System.Text.Json, IHttpClientFactory

---

## File Map

| File | Action |
|------|--------|
| `MinuteMind/MinuteMind.csproj` | Add LLamaSharp + Backend.Cpu NuGet references |
| `MinuteMind/Services/Contracts/IMinutesGeneratorService.cs` | Add `IProgress<string>?` parameter |
| `MinuteMind/Services/Implementations/MockMinutesGeneratorService.cs` | Update signature to match new interface |
| `MinuteMind/Services/Implementations/LlmMinutesGeneratorService.cs` | **Create** — download, load, infer, parse |
| `MinuteMind/ViewModels/ProcessingViewModel.cs` | Pass `IProgress<string>` into `GenerateAsync` |
| `MinuteMind/MauiProgram.cs` | Add `AddHttpClient()`, swap Mock → Llm service |

---

## Task 1: Add LLamaSharp NuGet Packages

**Files:**
- Modify: `MinuteMind/MinuteMind.csproj`

- [ ] **Step 1: Add NuGet package references**

Open `MinuteMind/MinuteMind.csproj`. Add these two lines inside the existing `<ItemGroup>` that contains the other `<PackageReference>` entries (after the `Whisper.net.Runtime` line):

```xml
<PackageReference Include="LLamaSharp" Version="0.20.0" />
<PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.20.0" />
```

The ItemGroup should now end with:
```xml
        <PackageReference Include="Whisper.net" Version="1.9.0" />
        <PackageReference Include="Whisper.net.Runtime" Version="1.9.0" />
        <PackageReference Include="LLamaSharp" Version="0.20.0" />
        <PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.20.0" />
    </ItemGroup>
```

- [ ] **Step 2: Restore packages**

```bash
cd "/Users/zimbadev/Documents/Workspace/Maui Projects/MinuteMind"
dotnet restore MinuteMind/MinuteMind.csproj
```

Expected: `Restore completed` with no errors. If `0.20.0` is not found, run:
```bash
dotnet package search LLamaSharp --take 5
```
and substitute the latest stable version for both packages (they must match).

- [ ] **Step 3: Verify build still compiles (no logic changes yet)**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug 2>&1 | tail -20
```

Expected: `Build succeeded` (warnings OK, errors not OK).

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/MinuteMind.csproj
git commit -m "chore: add LLamaSharp + Backend.Cpu packages for on-device LLM"
```

---

## Task 2: Update IMinutesGeneratorService Interface

**Files:**
- Modify: `MinuteMind/Services/Contracts/IMinutesGeneratorService.cs`

- [ ] **Step 1: Add IProgress parameter to the interface**

Replace the entire file contents:

```csharp
using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IMinutesGeneratorService
{
    Task<MeetingMinutes> GenerateAsync(
        List<TranscriptSegment> transcript,
        IProgress<string>? progress = null);
}
```

- [ ] **Step 2: Update MockMinutesGeneratorService to match**

Open `MinuteMind/Services/Implementations/MockMinutesGeneratorService.cs`. Replace the method signature (line 8) only — the body stays the same:

```csharp
public async Task<MeetingMinutes> GenerateAsync(List<TranscriptSegment> transcript, IProgress<string>? progress = null)
```

The full file should now be:
```csharp
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class MockMinutesGeneratorService : IMinutesGeneratorService
{
    public async Task<MeetingMinutes> GenerateAsync(List<TranscriptSegment> transcript, IProgress<string>? progress = null)
    {
        await Task.Delay(2000);

        return new MeetingMinutes
        {
            Attendees = ["John Doe", "Mary Smith", "Alex Rivera"],
            DiscussionPoints =
            [
                "Review of the Q4 roadmap and alignment on technical debt prioritization.",
                "Analysis of the current conversion funnel drop-off at the payment gateway.",
                "Initial brainstorming for the 2024 mobile redesign, focusing on accessibility."
            ],
            Decisions =
            [
                new Decision { Category = "Technical", Text = "Approved the shift to micro-services architecture for checkout by Q1." },
                new Decision { Category = "Budgeting", Text = "Allocated 15% of engineering capacity to high-priority security vulnerabilities." }
            ],
            ActionItems =
            [
                new ActionItem { Description = "Finalize the architecture diagram", Assignee = "John", DueDate = "Oct 27", IsCompleted = false },
                new ActionItem { Description = "Schedule follow-up with DevOps", Assignee = "Mary", DueDate = "Completed", IsCompleted = true },
                new ActionItem { Description = "Update Jira board with new priorities", Assignee = "Alex", DueDate = "Tomorrow", IsCompleted = false },
                new ActionItem { Description = "Prepare Q4 roadmap presentation", Assignee = "Alex", DueDate = "Friday", IsCompleted = false }
            ]
        };
    }
}
```

- [ ] **Step 3: Verify build — interface + mock must compile together**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug 2>&1 | tail -20
```

Expected: `Build succeeded`. If you see `CS0738` (interface mismatch) the method signature didn't match exactly — re-check the parameter names and types.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/Services/Contracts/IMinutesGeneratorService.cs \
        MinuteMind/Services/Implementations/MockMinutesGeneratorService.cs
git commit -m "feat: add IProgress<string> parameter to IMinutesGeneratorService"
```

---

## Task 3: Create LlmMinutesGeneratorService

**Files:**
- Create: `MinuteMind/Services/Implementations/LlmMinutesGeneratorService.cs`

This is the core of the feature. It has three responsibilities:
1. Download the GGUF model on first use (streaming, with progress)
2. Load + run inference via LLamaSharp
3. Parse the JSON response into `MeetingMinutes` with a safe fallback

- [ ] **Step 1: Create the file**

Create `MinuteMind/Services/Implementations/LlmMinutesGeneratorService.cs` with the following content:

```csharp
using System.Text;
using System.Text.Json;
using LLama;
using LLama.Common;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class LlmMinutesGeneratorService(IHttpClientFactory httpClientFactory) : IMinutesGeneratorService
{
    private const string ModelFileName = "qwen2.5-0.5b-instruct-q4_k_m.gguf";
    private const string ModelUrl = "https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF/resolve/main/qwen2.5-0.5b-instruct-q4_k_m.gguf";
    private const int MaxTranscriptChars = 6000;

    private string ModelPath => Path.Combine(FileSystem.AppDataDirectory, ModelFileName);

    public async Task<MeetingMinutes> GenerateAsync(
        List<TranscriptSegment> transcript,
        IProgress<string>? progress = null)
    {
        await EnsureModelDownloadedAsync(progress);

        progress?.Report("Loading AI model…");
        var modelParams = new ModelParams(ModelPath)
        {
            ContextSize = 4096u,
            GpuLayerCount = 0
        };

        using var weights = LLamaWeights.LoadFromFile(modelParams);
        var executor = new StatelessExecutor(weights, modelParams);

        progress?.Report("Generating minutes…");
        var prompt = BuildPrompt(transcript);
        var inferenceParams = new InferenceParams
        {
            MaxTokens = 1024,
            Temperature = 0.1f,
            AntiPrompts = ["<|im_end|>", "<|endoftext|>", "<|im_start|>"]
        };

        var sb = new StringBuilder();
        await foreach (var token in executor.InferAsync(prompt, inferenceParams))
        {
            sb.Append(token);
        }

        return ParseMinutes(sb.ToString());
    }

    private async Task EnsureModelDownloadedAsync(IProgress<string>? progress)
    {
        if (File.Exists(ModelPath))
            return;

        var tempPath = ModelPath + ".tmp";
        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var file = File.Create(tempPath);

            var buffer = new byte[81920];
            long downloaded = 0;
            int read;
            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, read));
                downloaded += read;
                if (totalBytes > 0)
                {
                    var pct = (int)(downloaded * 100 / totalBytes);
                    progress?.Report($"Downloading AI model ({pct}%)…");
                }
            }
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }

        File.Move(tempPath, ModelPath);
    }

    private static string BuildPrompt(List<TranscriptSegment> transcript)
    {
        const string system =
            "You are a meeting minutes assistant. Extract structured notes from the meeting transcript below.\n" +
            "Return ONLY a valid JSON object — no explanation, no markdown, no extra text.\n" +
            "Use this exact schema:\n" +
            "{\n" +
            "  \"Attendees\": [\"name or Speaker 1 if unknown\"],\n" +
            "  \"DiscussionPoints\": [\"concise bullet point\"],\n" +
            "  \"Decisions\": [{\"Category\": \"Technical|Process|Budget|Other\", \"Text\": \"decision made\"}],\n" +
            "  \"ActionItems\": [{\"Description\": \"task\", \"Assignee\": \"person or Unknown\", \"DueDate\": \"date or empty string\", \"IsCompleted\": false}]\n" +
            "}";

        var transcriptText = new StringBuilder();
        foreach (var seg in transcript)
        {
            var line = $"[{seg.Timestamp:mm\\:ss}] {seg.Speaker}: {seg.Text}\n";
            if (transcriptText.Length + line.Length > MaxTranscriptChars)
                break;
            transcriptText.Append(line);
        }

        return
            "<|im_start|>system\n" + system + "\n<|im_end|>\n" +
            "<|im_start|>user\n" + transcriptText + "<|im_end|>\n" +
            "<|im_start|>assistant\n";
    }

    private static MeetingMinutes ParseMinutes(string output)
    {
        try
        {
            var start = output.IndexOf('{');
            var end = output.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                var json = output[start..(end + 1)];
                var result = JsonSerializer.Deserialize<MeetingMinutes>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result is not null)
                    return result;
            }
        }
        catch
        {
            // fall through to fallback
        }

        return new MeetingMinutes
        {
            DiscussionPoints = ["Could not parse AI output — please edit manually."]
        };
    }
}
```

- [ ] **Step 2: Build to confirm the new file compiles**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug 2>&1 | tail -30
```

Expected: `Build succeeded`. Common errors and fixes:
- `CS0234: The type or namespace name 'LLama' does not exist` → packages not restored; run `dotnet restore` first
- `CS0246: StatelessExecutor` not found → check LLamaSharp version; in some versions it's `LLama.Executor.StatelessExecutor`, in others the using is `LLama` only
- `CS0246: InferenceParams` not found → it's in `LLama.Common`, add `using LLama.Common;` (already in file)

- [ ] **Step 3: Commit**

```bash
git add MinuteMind/Services/Implementations/LlmMinutesGeneratorService.cs
git commit -m "feat: add LlmMinutesGeneratorService with Qwen2.5 download + inference"
```

---

## Task 4: Update ProcessingViewModel to Pass Progress

**Files:**
- Modify: `MinuteMind/ViewModels/ProcessingViewModel.cs` (line 79)

Currently line 79 calls `minutesGenerator.GenerateAsync(segments)` without progress. We need to pass a `Progress<string>` that updates `Steps[2].Subtitle` live during download/load/inference.

- [ ] **Step 1: Replace the GenerateAsync call**

In `ProcessingViewModel.cs`, find this block (around lines 76–81):

```csharp
        // Step 3: Generate minutes
        Steps[2].Status = StepStatus.Active;
        Steps[2].Subtitle = "Analyzing transcript...";
        var minutes = await minutesGenerator.GenerateAsync(segments);
        Steps[2].Status = StepStatus.Completed;
        Steps[2].Subtitle = "Minutes ready";
```

Replace it with:

```csharp
        // Step 3: Generate minutes
        Steps[2].Status = StepStatus.Active;
        Steps[2].Subtitle = "Analyzing transcript...";
        var minutes = await minutesGenerator.GenerateAsync(
            segments,
            new Progress<string>(msg =>
            {
                MainThread.BeginInvokeOnMainThread(() => Steps[2].Subtitle = msg);
            }));
        Steps[2].Status = StepStatus.Completed;
        Steps[2].Subtitle = "Minutes ready";
```

- [ ] **Step 2: Build to confirm**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug 2>&1 | tail -20
```

Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add MinuteMind/ViewModels/ProcessingViewModel.cs
git commit -m "feat: wire IProgress<string> to ProcessingViewModel step 3 subtitle"
```

---

## Task 5: Register LlmMinutesGeneratorService in MauiProgram.cs

**Files:**
- Modify: `MinuteMind/MauiProgram.cs`

Two changes:
1. Add `builder.Services.AddHttpClient()` so `IHttpClientFactory` is available
2. Swap `MockMinutesGeneratorService` → `LlmMinutesGeneratorService`

- [ ] **Step 1: Add AddHttpClient() before the ViewModels block**

In `MauiProgram.cs`, find:
```csharp
        builder.Services.AddTransient<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<Plugin.Maui.Audio.IAudioManager>(AudioManager.Current);
```

Replace with:
```csharp
        builder.Services.AddTransient<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<Plugin.Maui.Audio.IAudioManager>(AudioManager.Current);
        builder.Services.AddHttpClient();
```

- [ ] **Step 2: Swap the minutes generator registration**

Find:
```csharp
        builder.Services.AddTransient<IMinutesGeneratorService, MockMinutesGeneratorService>();
```

Replace with:
```csharp
        builder.Services.AddTransient<IMinutesGeneratorService, LlmMinutesGeneratorService>();
```

- [ ] **Step 3: Build to confirm**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug 2>&1 | tail -20
```

Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```bash
git add MinuteMind/MauiProgram.cs
git commit -m "feat: register LlmMinutesGeneratorService + AddHttpClient in DI"
```

---

## Task 6: Deploy and Smoke Test on Device

This is an Android-first feature. The only meaningful test is running on a real ARM64 device connected via ADB.

- [ ] **Step 1: Connect device and verify ADB sees it**

```bash
adb devices
```

Expected output (example):
```
List of devices attached
R5CR80TVGDZ	device
```

Note your device serial. Replace `<SERIAL>` in all commands below.

- [ ] **Step 2: Full build + deploy**

```bash
dotnet build MinuteMind/MinuteMind.csproj -f net10.0-android -c Debug \
  -t:Run "-p:AdbTarget=-s <SERIAL>" 2>&1 | tail -40
```

Expected: App installs and launches on device.

- [ ] **Step 3: First-run download test**

On the device:
1. Tap "Record Meeting" and record ~10 seconds of speech
2. Stop recording → observe ProcessingPage
3. Step 3 subtitle should animate through: `"Downloading AI model (0%)…"` → `"…(47%)…"` → `"…(100%)…"` → `"Loading AI model…"` → `"Generating minutes…"` → `"Minutes ready"`
4. Download is ~300 MB — takes 2–5 minutes on WiFi. The app must stay alive.
5. After completion: MinutesPage opens with AI-generated (not hardcoded) minutes

- [ ] **Step 4: Second-run speed test**

Record another meeting. This time step 3 should skip straight to `"Loading AI model…"` (no download). The full step should complete in 20–40 seconds.

- [ ] **Step 5: Verify fallback path (optional)**

To test the fallback without real audio, temporarily in `LlmMinutesGeneratorService.ParseMinutes` change the JSON extraction logic to always return null, rebuild, deploy, record a meeting. MinutesPage should open with the single discussion point "Could not parse AI output — please edit manually." Revert the change after verifying.

---

## Troubleshooting

**`DllNotFoundException: libllama` on device**
LLamaSharp.Backend.Cpu's `.so` wasn't packaged. Check the APK:
```bash
cd MinuteMind/bin/Debug/net10.0-android/
ls -la *.apk
# then inspect:
unzip -l MinuteMind.apk | grep -i llama
```
If missing, add to `.csproj`:
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
  <AndroidNativeLibrary Include="$(NuGetPackageRoot)llamasharp.backend.cpu\0.20.0\runtimes\android-arm64\native\libllama.so">
    <Abi>arm64-v8a</Abi>
  </AndroidNativeLibrary>
</ItemGroup>
```
Adjust the path to match where NuGet caches the package on your machine.

**App killed during 300 MB download (background)**
Android may kill the app during a long network operation. If this happens, add a foreground service to `ProcessingPage`. For now, keep the screen on and don't switch apps during first-run download.

**`OutOfMemoryException` when loading model**
The 300 MB model + 4096-token context requires ~600 MB RAM. Low-end devices may fail. If this happens, switch to `ContextSize = 2048u` and reduce `MaxTranscriptChars` to 3000.

**Model output is empty or gibberish**
1. Verify the model downloaded completely: `adb shell ls -la /data/data/com.companyname.minutemind/files/`
2. File should be ~310 MB. If smaller, delete it and retry.
3. Try reducing temperature to `0.05f`.

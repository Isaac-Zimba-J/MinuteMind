# Local LLM Meeting Minutes Generation — Design Spec

> Last updated: 2026-06-10  
> Status: Approved — ready for implementation planning

---

## Goal

Replace `MockMinutesGeneratorService` with a real on-device LLM that reads the Whisper transcript and produces structured meeting minutes matching the existing `MeetingMinutes` model and MinutesPage UI. No cloud, no API key, free forever.

---

## Model Choice

**Qwen2.5 0.5B Instruct Q4_K_M (GGUF)**

| Property | Value |
|----------|-------|
| Download size | ~300 MB |
| License | Apache 2.0 — free, commercial use OK |
| Source | Official Qwen repo on Hugging Face |
| Download URL | `https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF/resolve/main/qwen2.5-0.5b-instruct-q4_k_m.gguf` |
| Cached path | `{AppDataDirectory}/qwen2.5-0.5b-instruct-q4_k_m.gguf` |
| Inference time | ~20–40s on modern Android ARM64 (CPU only) |
| Context window | 4096 tokens (enough for ~1 hour of transcript) |

---

## Architecture

### Files changed

| File | Change |
|------|--------|
| `Services/Contracts/IMinutesGeneratorService.cs` | Add `IProgress<string>?` parameter to `GenerateAsync` |
| `Services/Implementations/LlmMinutesGeneratorService.cs` | **New** — full implementation |
| `Services/Implementations/MockMinutesGeneratorService.cs` | Add `IProgress<string>?` parameter to match updated interface |
| `ViewModels/ProcessingViewModel.cs` | Pass `IProgress<string>` for step 3 subtitle updates |
| `MauiProgram.cs` | Swap `MockMinutesGeneratorService` → `LlmMinutesGeneratorService` |

### NuGet packages to add

| Package | Purpose |
|---------|---------|
| `LLamaSharp` (latest stable) | .NET wrapper for llama.cpp, includes Android ARM64 native libs |
| `LLamaSharp.Backend.Cpu` (matching version) | CPU inference backend with Android ARM64 `.so` |

### Pipeline (unchanged externally)

```
AudioRecorderService
    → [WAV file]
LocalTranscriptionService (Whisper)
    → List<TranscriptSegment>
LlmMinutesGeneratorService (Qwen2.5 0.5B)   ← this spec
    → MeetingMinutes
MeetingRepository.SaveAsync()
    → NavigateTo(MinutesPage)
```

---

## Interface Change

```csharp
// Services/Contracts/IMinutesGeneratorService.cs
public interface IMinutesGeneratorService
{
    Task<MeetingMinutes> GenerateAsync(
        List<TranscriptSegment> transcript,
        IProgress<string>? progress = null);
}
```

The `progress` parameter is optional (default null) so `MockMinutesGeneratorService` requires only a trivial update and existing call sites without progress still compile.

---

## LlmMinutesGeneratorService Design

### Model lifecycle

1. `GenerateAsync` is called
2. Check if `{AppDataDirectory}/qwen2.5-0.5b-instruct-q4_k_m.gguf` exists
3. If not: stream-download from Hugging Face, report progress as `"Downloading AI model ({pct}%)…"`
4. Load model with `LLamaWeights.LoadFromFile` — report `"Loading AI model…"`
5. Run inference — report `"Generating minutes…"`
6. Parse JSON response → `MeetingMinutes`
7. Return result (model instance is disposed after each call — no singleton to avoid memory pressure on Android)

### Download

- Uses `IHttpClientFactory` (already registered) — named client `"llm-download"`
- Streams to a temp file first (`*.tmp`), renames to final path only on success — prevents corrupt partial files
- If download throws: delete temp file, rethrow — ProcessingViewModel's existing error handler shows the alert

### Prompt (Qwen2.5 ChatML format)

```
<|im_start|>system
You are a meeting minutes assistant. Extract structured notes from the meeting transcript below.
Return ONLY a valid JSON object — no explanation, no markdown, no extra text.
Use this exact schema:
{
  "Attendees": ["name or Speaker 1 if unknown"],
  "DiscussionPoints": ["concise bullet point"],
  "Decisions": [{"Category": "Technical|Process|Budget|Other", "Text": "decision made"}],
  "ActionItems": [{"Description": "task", "Assignee": "person or Unknown", "DueDate": "date or empty string", "IsCompleted": false}]
}
<|im_end|>
<|im_start|>user
{formatted transcript}
<|im_end|>
<|im_start|>assistant
```

Transcript is formatted as:
```
[mm:ss] Speaker 1: text of segment
[mm:ss] Speaker 1: next segment
```

### JSON parsing

Extract content between first `{` and last `}` in model output (handles any stray tokens around the JSON), then `JsonSerializer.Deserialize<MeetingMinutes>` with `PropertyNameCaseInsensitive = true`.

If parsing fails: return a fallback `MeetingMinutes` with a single discussion point `"Could not parse AI output — please edit manually."` so the user still lands on MinutesPage with something to work with.

### Inference parameters

```csharp
ContextSize    = 4096
GpuLayerCount  = 0       // CPU-only — Android has no GPU compute API for llama.cpp
MaxTokens      = 1024    // enough for full JSON output
Temperature    = 0.1f    // low randomness for structured output
AntiPrompts    = ["<|im_end|>", "<|endoftext|>", "<|im_start|>"]
```

---

## ProcessingViewModel Change

Step 3 progress is passed to `GenerateAsync`:

```csharp
var minutes = await minutesGenerator.GenerateAsync(
    segments,
    new Progress<string>(msg => Steps[2].Subtitle = msg));
```

Step 3 subtitle will now show live status:
- `"Downloading AI model (47%)…"` (first run only)
- `"Loading AI model…"`
- `"Generating minutes…"`

---

## Error Handling

| Failure | Result |
|---------|--------|
| Download fails (no internet) | Exception caught by ProcessingViewModel → alert: "Could not download AI model. Please check your internet connection and try again." Transcript already saved. |
| JSON parse fails | Fallback `MeetingMinutes` with single discussion point asking user to edit manually |
| Model load fails | Exception propagates → ProcessingViewModel alert |
| Transcript too long (>4096 tokens) | Truncate transcript to last N segments that fit before sending to model |

---

## What This Does NOT Change

- Whisper transcription — unchanged
- MinutesPage UI — unchanged (already matches `MeetingMinutes` schema)
- EditMinutesPage — unchanged (user can correct AI output)
- ExportPage — unchanged
- SQLite schema — unchanged (minutes still stored as JSON string)

using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class LocalTranscriptionService : ITranscriptionService
{
    public async Task<List<TranscriptSegment>> TranscribeAsync(string audioPath, IProgress<string>? progress = null)
    {
        progress?.Report("Loading model...");
        await Task.Delay(1000);

        progress?.Report("Transcribing audio...");
        await Task.Delay(2000);

        return
        [
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromSeconds(4),
                Speaker = "Speaker 1",
                Text = "Alright everyone, let's dive into the Q3 product roadmap. We've seen significant traction with the new typography system."
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromSeconds(48),
                Speaker = "Speaker 2",
                Text = "The team is loving the Plus Jakarta Sans implementation. However, we're seeing some layout shifting on smaller devices."
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(22),
                Speaker = "Speaker 1",
                Text = "That's a fair point. We should adjust the tracking for those specific breakpoints. Can we look at the fluid typography scale again?"
            },
            new TranscriptSegment
            {
                Timestamp = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(15),
                Speaker = "Speaker 2",
                Text = "Absolutely. I'll sync with the UI developers this afternoon. The performance metrics for the transcription engine are peaking at 98% accuracy."
            }
        ];
    }
}

using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface ITranscriptionService
{
    Task<List<TranscriptSegment>> TranscribeAsync(string audioPath, IProgress<string>? progress = null);
}

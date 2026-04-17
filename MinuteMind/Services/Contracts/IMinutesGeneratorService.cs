using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IMinutesGeneratorService
{
    Task<MeetingMinutes> GenerateAsync(List<TranscriptSegment> transcript);
}

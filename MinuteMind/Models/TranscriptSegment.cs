namespace MinuteMind.Models;

public class TranscriptSegment
{
    public TimeSpan Timestamp { get; set; }
    public string Speaker { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

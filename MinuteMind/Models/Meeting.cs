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

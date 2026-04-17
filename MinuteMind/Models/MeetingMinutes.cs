namespace MinuteMind.Models;

public class MeetingMinutes
{
    public List<string> Attendees { get; set; } = [];
    public List<string> DiscussionPoints { get; set; } = [];
    public List<Decision> Decisions { get; set; } = [];
    public List<ActionItem> ActionItems { get; set; } = [];
}

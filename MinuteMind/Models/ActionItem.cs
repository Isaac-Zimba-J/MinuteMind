namespace MinuteMind.Models;

public class ActionItem
{
    public string Description { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

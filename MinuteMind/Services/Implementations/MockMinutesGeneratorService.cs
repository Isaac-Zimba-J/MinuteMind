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

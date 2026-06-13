using MinuteMind.Data;
using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class MeetingRepository(MinuteMindDatabase database) : IMeetingRepository
{
    public Task<List<Meeting>> GetAllAsync() => database.GetAllMeetingsAsync();
    public Task<List<Meeting>> GetRecentAsync(int count) => database.GetRecentMeetingsAsync(count);
    public Task<Meeting?> GetByIdAsync(int id) => database.GetMeetingAsync(id);
    public Task<int> SaveAsync(Meeting meeting) => database.SaveMeetingAsync(meeting);
    public Task<int> DeleteAsync(Meeting meeting) => database.DeleteMeetingAsync(meeting);
    public Task DeleteAllAsync() => database.DeleteAllMeetingsAsync();
}

using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IMeetingRepository
{
    Task<List<Meeting>> GetAllAsync();
    Task<List<Meeting>> GetRecentAsync(int count);
    Task<Meeting?> GetByIdAsync(int id);
    Task<int> SaveAsync(Meeting meeting);
    Task<int> DeleteAsync(Meeting meeting);
}

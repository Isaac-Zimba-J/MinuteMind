using MinuteMind.Models;
using SQLite;

namespace MinuteMind.Data;

public class MinuteMindDatabase
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;

    public MinuteMindDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "minutemind.db3");
    }

    private async Task InitAsync()
    {
        if (_db is not null)
            return;

        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Meeting>();
    }

    public async Task<List<Meeting>> GetAllMeetingsAsync()
    {
        await InitAsync();
        return await _db!.Table<Meeting>().OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task<List<Meeting>> GetRecentMeetingsAsync(int count)
    {
        await InitAsync();
        return await _db!.Table<Meeting>().OrderByDescending(m => m.CreatedAt).Take(count).ToListAsync();
    }

    public async Task<Meeting?> GetMeetingAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<Meeting>().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> SaveMeetingAsync(Meeting meeting)
    {
        await InitAsync();
        meeting.UpdatedAt = DateTime.UtcNow;

        if (meeting.Id == 0)
        {
            meeting.CreatedAt = DateTime.UtcNow;
            if (meeting.Date == default)
                meeting.Date = DateTime.UtcNow;
            return await _db!.InsertAsync(meeting);
        }

        return await _db!.UpdateAsync(meeting);
    }

    public async Task<int> DeleteMeetingAsync(Meeting meeting)
    {
        await InitAsync();
        return await _db!.DeleteAsync(meeting);
    }

    public async Task DeleteAllMeetingsAsync()
    {
        await InitAsync();
        await _db!.DeleteAllAsync<Meeting>();
    }
}

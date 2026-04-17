using MinuteMind.Models;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    public async Task<string> ExportAsync(Meeting meeting)
    {
        await Task.Delay(500);
        var path = Path.Combine(FileSystem.AppDataDirectory, $"{meeting.Title.Replace(" ", "_")}.pdf");
        await File.WriteAllBytesAsync(path, []);
        return path;
    }
}

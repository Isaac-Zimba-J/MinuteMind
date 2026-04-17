using MinuteMind.Models;

namespace MinuteMind.Services.Contracts;

public interface IPdfExportService
{
    Task<string> ExportAsync(Meeting meeting);
}

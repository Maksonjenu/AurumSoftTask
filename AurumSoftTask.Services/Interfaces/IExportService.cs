
namespace AurumSoftTask.Services.Interfaces
{
    public interface IExportService
    {
        Task ExportAsync<T>(T data, string filePath);
    }
}


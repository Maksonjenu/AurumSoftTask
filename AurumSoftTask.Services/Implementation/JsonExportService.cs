using AurumSoftTask.Services.Interfaces;
using System.Text.Json;

namespace AurumSoftTask.Services.Implementation
{
    public class JsonExportService : IExportService
    {
        public async Task ExportAsync<T>(T data, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using FileStream createStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(createStream, data, options);
        }
    }
}

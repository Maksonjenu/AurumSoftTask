using AurumSoftTask.Core.Models;

namespace AurumSoftTask.Services.Interfaces
{
    public interface ICsvParser
    {
        (List<CsvRow> Rows, List<ValidationError> ParseErrors) Parse(string filePath);
    }
}

using AurumSoftTask.Core.Models;

namespace AurumSoftTask.Services.Interfaces
{
    public interface IWellValidator
    {
        (List<CsvRow> ValidRows, List<ValidationError> ValidationErrors) Validate(List<CsvRow> rows);

    }
}

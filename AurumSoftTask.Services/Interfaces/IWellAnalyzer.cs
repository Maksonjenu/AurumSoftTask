
using AurumSoftTask.Core.Models;

namespace AurumSoftTask.Services.Interfaces
{
    public interface IWellAnalyzer
    {
        List<WellSummary> CalculateSummary(List<Well> validRows);

    }
}

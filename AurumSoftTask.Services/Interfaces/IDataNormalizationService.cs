
namespace AurumSoftTask.Services.Interfaces
{
    public interface IDataNormalizationService
    {
        Task<Dictionary<string, string>> NormalizeRockNamesAsync(IEnumerable<string> rockNames);
    }
}

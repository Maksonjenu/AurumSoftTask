using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;

namespace AurumSoftTask.Services.Implementation;

public class WellAnalyzer : IWellAnalyzer
{
    public List<WellSummary> CalculateSummary(List<CsvRow> validRows)
    {
        var summaries = new List<WellSummary>();

        var groupedByWell = validRows.GroupBy(r => r.WellId);

        foreach (var group in groupedByWell)
        {
            var wellId = group.Key;
            var intervalsCount = group.Count();
            var totalDepth = group.Max(r => r.DepthTo); //pick just most deep point of well

            double totalThickness = group.Sum(r => r.DepthTo - r.DepthFrom); // total thickness of all intervals in the well
            double weightedPorosity = totalThickness == 0 // if TT is 0, no divide
                ? 0
                : group.Sum(r => r.Porosity * (r.DepthTo - r.DepthFrom)) / totalThickness; // sum of (porosity * thickness) for each interval
                                                                                           // divided by total thickness to get weighted average porosity

            // take most common rock type
            // by total thickness of intervals of that rock type
            var mostCommonRock = group
                .GroupBy(r => r.Rock) // sort by name
                .Select(rockGroup => new
                {
                    RockName = rockGroup.Key,
                    TotalRockThickness = rockGroup.Sum(r => r.DepthTo - r.DepthFrom) // calc TT for each rock type
                })
                .MaxBy(x => x.TotalRockThickness)? // take most thick rock type
                .RockName ?? "Unknown"; // no rock types in well

            summaries.Add(new WellSummary(wellId, totalDepth, intervalsCount, weightedPorosity, mostCommonRock));
        }

        return summaries.OrderBy(s => s.WellId).ToList();
    }
}
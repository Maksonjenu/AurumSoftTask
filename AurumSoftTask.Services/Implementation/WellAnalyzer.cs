using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;

namespace AurumSoftTask.Services.Implementation;

public class WellAnalyzer : IWellAnalyzer
{
    public List<WellSummary> CalculateSummary(List<Well> wells)
    {
        var summaries = new List<WellSummary>();

        foreach (var well in wells)
        {
            var intervalsCount = well.Intervals.Count;
            var totalDepth = well.Intervals.Max(r => r.DepthTo); //pick just most deep point of well

            double totalThickness = well.Intervals.Sum(r => r.DepthTo - r.DepthFrom); // total thickness of all intervals in the well
            double weightedPorosity = totalThickness == 0 // if TT is 0, no divide
                ? 0
                : well.Intervals.Sum(r => r.Porosity * (r.DepthTo - r.DepthFrom)) / totalThickness; // sum of (porosity * thickness) for each interval
                                                                                           // divided by total thickness to get weighted average porosity

            // take most common rock type
            // by total thickness of intervals of that rock type
            var mostCommonRock = well.Intervals
                .GroupBy(r => r.Rock) // sort by name
                .MaxBy(g => g.Sum(i => i.DepthTo - i.DepthFrom))? // take most thick rock type
                .Key ?? "Unknown"; // no rock types in well

            summaries.Add(new WellSummary(well.WellId, totalDepth, intervalsCount, weightedPorosity, mostCommonRock));
        }

        return summaries.OrderBy(s => s.WellId).ToList();
    }
}
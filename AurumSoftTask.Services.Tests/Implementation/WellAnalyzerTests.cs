using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Implementations;
using AurumSoftTask.Services.Interfaces;
using NUnit.Framework;

namespace AurumSoftTask.Services.Tests.Implementation;

[TestFixture]
public class WellAnalyzerTests
{
    private IWellAnalyzer _analyzer = null!;

    [SetUp]
    public void SetUp()
    {
        _analyzer = new WellAnalyzer();
    }

    private static CsvRow Row(int line, string wellId, double x, double y, double depthFrom, double depthTo, string rock, double porosity)
        => new(line, wellId, x, y, depthFrom, depthTo, rock, porosity);

    [Test]
    public void CalculateSummary_EmptyList_ReturnsEmptyList()
    {
        var result = _analyzer.CalculateSummary(new List<CsvRow>());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CalculateSummary_SingleWellSingleInterval_ReturnsOneSummary()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", 0.18)
        };

        var result = _analyzer.CalculateSummary(rows);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].WellId, Is.EqualTo("A-001"));
        Assert.That(result[0].TotalDepth, Is.EqualTo(10));
        Assert.That(result[0].IntervalCount, Is.EqualTo(1));
        Assert.That(result[0].AveragePorosity, Is.EqualTo(0.18).Within(0.001));
        Assert.That(result[0].TopRockType, Is.EqualTo("Sandstone"));
    }

    [Test]
    public void CalculateSummary_ExampleData_ReturnsCorrectSummaries()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.10, 55.20, 0, 10, "Sandstone", 0.18),
            Row(3, "A-001", 82.10, 55.20, 10, 25, "Limestone", 0.07),
            Row(4, "A-002", 90.00, 60.00, 0, 15, "Shale", 0.04),
            Row(5, "A-002", 90.00, 60.00, 15, 40, "Sandstone", 0.22),
            Row(6, "A-003", 100.10, 72.50, 0, 5, "Sandstone", 0.19)
        };

        var result = _analyzer.CalculateSummary(rows);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Select(s => s.WellId).ToList(), Is.EqualTo(new[] { "A-001", "A-002", "A-003" }));

        var a001 = result.First(s => s.WellId == "A-001");
        Assert.That(a001.TotalDepth, Is.EqualTo(25));
        Assert.That(a001.IntervalCount, Is.EqualTo(2));
        double expectedPorosityA001 = (0.18 * 10 + 0.07 * 15) / 25;
        Assert.That(a001.AveragePorosity, Is.EqualTo(expectedPorosityA001).Within(0.001));
        Assert.That(a001.TopRockType, Is.EqualTo("Limestone"));

        var a002 = result.First(s => s.WellId == "A-002");
        Assert.That(a002.TotalDepth, Is.EqualTo(40));
        Assert.That(a002.IntervalCount, Is.EqualTo(2));
        Assert.That(a002.TopRockType, Is.EqualTo("Sandstone"));

        var a003 = result.First(s => s.WellId == "A-003");
        Assert.That(a003.TotalDepth, Is.EqualTo(5));
        Assert.That(a003.IntervalCount, Is.EqualTo(1));
        Assert.That(a003.AveragePorosity, Is.EqualTo(0.19).Within(0.001));
        Assert.That(a003.TopRockType, Is.EqualTo("Sandstone"));
    }

    [Test]
    public void CalculateSummary_MultipleWells_OrderedByWellId()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "Z-001", 0, 0, 0, 5, "Shale", 0.1),
            Row(3, "A-001", 0, 0, 0, 10, "Sandstone", 0.2),
            Row(4, "M-001", 0, 0, 0, 8, "Limestone", 0.15)
        };

        var result = _analyzer.CalculateSummary(rows);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].WellId, Is.EqualTo("A-001"));
        Assert.That(result[1].WellId, Is.EqualTo("M-001"));
        Assert.That(result[2].WellId, Is.EqualTo("Z-001"));
    }
}

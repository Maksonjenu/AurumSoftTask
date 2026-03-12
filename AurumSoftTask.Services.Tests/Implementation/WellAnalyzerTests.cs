using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Implementation;
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
        var result = _analyzer.CalculateSummary(new List<Well>());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CalculateSummary_SingleWellSingleInterval_ReturnsOneSummary()
    {
        var wells = new List<Well>
        {
            new Well()
            {
                WellId = "A-001",
                X = 82.1,
                Y = 55.2,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 10,
                        Rock = "Sandstone",
                        Porosity = 0.18
                    }
                }

            }
        };

        var result = _analyzer.CalculateSummary(wells);

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
        var wells = new List<Well>
        {
            new Well
            {
                WellId = "A-001",
                X = 82.10,
                Y = 55.20,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 10,
                        Rock = "Sandstone",
                        Porosity = 0.18
                    },
                    new Interval
                    {
                        DepthFrom = 10,
                        DepthTo = 25,
                        Rock = "Limestone",
                        Porosity = 0.07
                    }
                }
            },
            new Well
            {
                WellId = "A-002",
                X = 90.00,
                Y = 60.00,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 15,
                        Rock = "Shale",
                        Porosity = 0.04
                    },
                    new Interval
                    {
                        DepthFrom = 15,
                        DepthTo = 40,
                        Rock = "Sandstone",
                        Porosity = 0.22
                    }
                }
            },
            new Well
            {
                WellId = "A-003",
                X = 100.10,
                Y = 72.50,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 5,
                        Rock = "Sandstone",
                        Porosity = 0.19
                    }
                }
            }
        };

        var result = _analyzer.CalculateSummary(wells);

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
        var wells = new List<Well>
        {
            new Well
            {
                WellId = "Z-001",
                X = 0,
                Y = 0,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 5,
                        Rock = "Shale",
                        Porosity = 0.1
                    }
                }
            },
            new Well
            {
                WellId = "A-001",
                X = 0,
                Y = 0,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 10,
                        Rock = "Sandstone",
                        Porosity = 0.2
                    }
                }
            },
            new Well
            {
                WellId = "M-001",
                X = 0,
                Y = 0,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 8,
                        Rock = "Limestone",
                        Porosity = 0.15
                    }
                }
            }
        };

        var result = _analyzer.CalculateSummary(wells);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].WellId, Is.EqualTo("A-001"));
        Assert.That(result[1].WellId, Is.EqualTo("M-001"));
        Assert.That(result[2].WellId, Is.EqualTo("Z-001"));
    }
}

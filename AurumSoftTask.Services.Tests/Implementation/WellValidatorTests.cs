using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Implementation;
using AurumSoftTask.Services.Interfaces;
using NUnit.Framework;

namespace AurumSoftTask.Services.Tests.Implementation;

[TestFixture]
public class WellValidatorTests
{
    private IWellValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new WellValidator();
    }

    private static CsvRow Row(int line, string wellId, double x, double y, double depthFrom, double depthTo, string rock, double porosity)
        => new(line, wellId, x, y, depthFrom, depthTo, rock, porosity);

    [Test]
    public void Validate_ValidRows_ReturnsAllAsValid_NoErrors()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", 0.18),
            Row(3, "A-001", 82.1, 55.2, 10, 25, "Limestone", 0.07),
            Row(4, "A-002", 90, 60, 0, 15, "Shale", 0.04)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Is.Empty);
        Assert.That(validRows, Has.Count.EqualTo(3));
        Assert.That(validRows.Select(r => r.WellId).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void Validate_NegativeDepthFrom_ReturnsError()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, -5, 10, "Sandstone", 0.18)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Has.Count.EqualTo(1));
        Assert.That(validationErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.NegativeDepth));
        Assert.That(validRows, Is.Empty);
    }

    [Test]
    public void Validate_DepthFromGreaterOrEqualDepthTo_ReturnsError()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 10, 10, "Sandstone", 0.18)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Has.Count.EqualTo(1));
        Assert.That(validationErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.InvalidDepthRange));
        Assert.That(validRows, Is.Empty);
    }

    [Test]
    public void Validate_PorosityOutOfRange_ReturnsError()
    {
        var rowsNegative = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", -0.1)
        };
        var (_, errorsNeg) = _validator.Validate(rowsNegative);
        Assert.That(errorsNeg, Has.Count.EqualTo(1));
        Assert.That(errorsNeg[0].ErrorType, Is.EqualTo(ValidationErrorType.InvalidPorosityValue));

        var rowsOverOne = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", 1.5)
        };
        var (_, errorsOver) = _validator.Validate(rowsOverOne);
        Assert.That(errorsOver, Has.Count.EqualTo(1));
        Assert.That(errorsOver[0].ErrorType, Is.EqualTo(ValidationErrorType.InvalidPorosityValue));
    }

    [Test]
    public void Validate_EmptyRock_ReturnsError()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "", 0.18)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Has.Count.EqualTo(1));
        Assert.That(validationErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.MissingRockType));
        Assert.That(validRows, Is.Empty);
    }

    [Test]
    public void Validate_OverlappingIntervalsSameWell_ReturnsError()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", 0.18),
            Row(3, "A-001", 82.1, 55.2, 5, 15, "Limestone", 0.07)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Has.Count.EqualTo(1));
        Assert.That(validationErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.OverlappingIntervals));
        Assert.That(validRows, Has.Count.EqualTo(1));
        Assert.That(validRows[0].DepthFrom, Is.EqualTo(0));
    }

    [Test]
    public void Validate_AdjacentIntervalsNoOverlap_Valid()
    {
        var rows = new List<CsvRow>
        {
            Row(2, "A-001", 82.1, 55.2, 0, 10, "Sandstone", 0.18),
            Row(3, "A-001", 82.1, 55.2, 10, 20, "Limestone", 0.07)
        };

        var (validRows, validationErrors) = _validator.Validate(rows);

        Assert.That(validationErrors, Is.Empty);
        Assert.That(validRows, Has.Count.EqualTo(2));
    }
}

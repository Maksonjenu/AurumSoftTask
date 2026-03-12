using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Implementation;
using AurumSoftTask.Services.Interfaces;
using NUnit.Framework;

namespace AurumSoftTask.Services.Tests.Implementation;

[TestFixture]
public class CsvParserTests
{
    private ICsvParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new CsvParser();
    }

    private static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    [Test]
    public void Parse_ValidCsv_ReturnsRows_NoErrors()
    {
        var path = GetTestDataPath("valid_sample.csv");
        Assume.That(File.Exists(path), $"TestData file not found: {path}");

        var (rows, parseErrors) = _parser.Parse(path);

        Assert.That(parseErrors, Is.Empty);
        Assert.That(rows, Has.Count.EqualTo(5));

        var first = rows.First(r => r.WellId == "A-001" && r.DepthFrom == 0);
        Assert.That(first.LineNumber, Is.GreaterThan(0));
        Assert.That(first.WellId, Is.EqualTo("A-001"));
        Assert.That(first.X, Is.EqualTo(82.10).Within(0.001));
        Assert.That(first.Y, Is.EqualTo(55.20).Within(0.001));
        Assert.That(first.DepthFrom, Is.EqualTo(0));
        Assert.That(first.DepthTo, Is.EqualTo(10));
        Assert.That(first.Rock, Is.EqualTo("Sandstone"));
        Assert.That(first.Porosity, Is.EqualTo(0.18).Within(0.001));

        var last = rows.First(r => r.WellId == "A-003");
        Assert.That(last.DepthTo, Is.EqualTo(5));
        Assert.That(last.Rock, Is.EqualTo("Sandstone"));
        Assert.That(last.Porosity, Is.EqualTo(0.19).Within(0.001));
    }

    [Test]
    public void Parse_InvalidColumnCount_ReturnsParseErrors()
    {
        var path = GetTestDataPath("invalid_columns.csv");
        Assume.That(File.Exists(path), $"TestData file not found: {path}");

        var (rows, parseErrors) = _parser.Parse(path);

        Assert.That(parseErrors, Has.Count.EqualTo(1));
        Assert.That(parseErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.IncorrectColumnCount));
        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].WellId, Is.EqualTo("A-002"));
    }

    [Test]
    public void Parse_MissingWellId_ReturnsParseErrors()
    {
        var path = GetTestDataPath("invalid_columns_no_id.csv");
        Assume.That(File.Exists(path), $"TestData file not found: {path}");

        var (rows, parseErrors) = _parser.Parse(path);

        Assert.That(parseErrors, Has.Count.EqualTo(2));
        Assert.That(parseErrors[0].ErrorType, Is.EqualTo(ValidationErrorType.MissingWellId));
        Assert.That(parseErrors[1].ErrorType, Is.EqualTo(ValidationErrorType.IncorrectColumnCount));
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(parseErrors[0].WellId, Is.EqualTo("Unknown"));
    }

    [Test]
    public void Parse_InvalidNumericField_ReturnsParseErrors()
    {
        var path = GetTestDataPath("invalid_numbers.csv");
        Assume.That(File.Exists(path), $"TestData file not found: {path}");

        var (rows, parseErrors) = _parser.Parse(path);

        Assert.That(parseErrors, Has.Count.AtLeast(1));
        Assert.That(parseErrors.Any(e => e.ErrorType == ValidationErrorType.NumericFieldParseError), Is.True);
        Assert.That(rows, Is.Empty);
    }

    [Test]
    public void Parse_EmptyOrWhitespaceLines_Skipped()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(tempPath, new[]
            {
                "WellId;X;Y;DepthFrom;DepthTo;Rock;Porosity",
                "",
                "A-001;1;2;0;10;Sandstone;0.18",
                "   ",
                "A-002;3;4;0;5;Shale;0.04"
            });

            var (rows, parseErrors) = _parser.Parse(tempPath);

            Assert.That(parseErrors, Is.Empty);
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(r => r.WellId), Is.EquivalentTo(new[] { "A-001", "A-002" }));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Test]
    public void Parse_TrailingEmptyLines_Skipped()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(tempPath, new[]
            {
                "WellId;X;Y;DepthFrom;DepthTo;Rock;Porosity",
                "A-001;1;2;0;10;Sandstone;0.18",
                "",
                "   "
            });

            var (rows, parseErrors) = _parser.Parse(tempPath);

            Assert.That(parseErrors, Is.Empty);
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].WellId, Is.EqualTo("A-001"));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Test]
    public void Parse_HeaderLine_Skipped()
    {
        var path = GetTestDataPath("valid_sample.csv");
        Assume.That(File.Exists(path), $"TestData file not found: {path}");

        var (rows, parseErrors) = _parser.Parse(path);

        Assert.That(parseErrors, Is.Empty);
        Assert.That(rows.Any(r => r.WellId == "WellId"), Is.False);
        Assert.That(rows, Has.Count.EqualTo(5));
    }
}

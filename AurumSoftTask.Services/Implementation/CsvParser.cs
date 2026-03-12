using System.Globalization;
using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;

namespace AurumSoftTask.Services.Implementation
{
    public class CsvParser : ICsvParser
    {
        public (List<CsvRow> Rows, List<ValidationError> ParseErrors) Parse(string filePath)
        {
            var rows = new List<CsvRow>();
            var errors = new List<ValidationError>();

            // first line header - skip
            var lines = File.ReadLines(filePath).Select((text, index) => new { text, lineNumber = index + 1 }).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.text)) continue;

                var parts = line.text.Split(';');

                if (string.IsNullOrWhiteSpace(parts[0]))
                {
                    errors.Add(CreateError(
                        line.lineNumber,
                        parts[0],
                        ValidationErrorType.MissingWellId,
                        ValidationErrorType.MissingWellId.GetDescription()
                        ));
                }

                if (parts.Length != 7)
                {

                    errors.Add(CreateError(
                        line.lineNumber,
                        parts.FirstOrDefault(),
                        ValidationErrorType.IncorrectColumnCount,
                        ValidationErrorType.IncorrectColumnCount.GetDescription()

                        ));

                    continue; //BUG: maybe it shoudnt skip parse, but include ALL file errors
                }

                // safe parse with invariant culture for decimal separator
                bool isXParsed = double.TryParse(parts[1], CultureInfo.InvariantCulture, out double x);
                bool isYParsed = double.TryParse(parts[2], CultureInfo.InvariantCulture, out double y);
                bool isDepthFromParsed = double.TryParse(parts[3], CultureInfo.InvariantCulture, out double depthFrom);
                bool isDepthToParsed = double.TryParse(parts[4], CultureInfo.InvariantCulture, out double depthTo);
                bool isPorosityParsed = double.TryParse(parts[6], CultureInfo.InvariantCulture, out double porosity);

                if (!isXParsed || !isYParsed || !isDepthFromParsed || !isDepthToParsed || !isPorosityParsed)
                {
                    errors.Add(CreateError(
                        line.lineNumber,
                        parts[0],
                        ValidationErrorType.NumericFieldParseError,
                        ValidationErrorType.NumericFieldParseError.GetDescription()

                        ));
                    continue;
                }

                rows.Add(new CsvRow(line.lineNumber, parts[0], x, y, depthFrom, depthTo, parts[5], porosity));
            }

            return (rows, errors);
        }

        private ValidationError CreateError(int line, string? wellId, ValidationErrorType errorType, string details) => new()
        {
            LineWithError = line,
            WellId = string.IsNullOrWhiteSpace(wellId) ? "Unknown" : wellId,
            ErrorDetails = details,
            ErrorType = errorType
        };

    }
}

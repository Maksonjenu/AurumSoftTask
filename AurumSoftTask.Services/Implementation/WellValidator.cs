using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;

namespace AurumSoftTask.Services.Implementations;

public class WellValidator : IWellValidator
{
    public (List<CsvRow> ValidRows, List<ValidationError> ValidationErrors) Validate(List<CsvRow> rows)
    {
        var validRows = new List<CsvRow>();
        var errors = new List<ValidationError>();
        var structurallyValidRows = new List<CsvRow>();

        // rule check for each row
        foreach (var row in rows)
        {
            if (row.DepthFrom < 0)
                errors.Add(CreateError(
                    row, 
                    ValidationErrorType.NegativeDepth,
                    $"{ValidationErrorType.NegativeDepth.GetDescription()} (DepthFrom: {row.DepthFrom})."
                    ));
            else if (row.DepthFrom >= row.DepthTo)
                errors.Add(CreateError(
                    row, 
                    ValidationErrorType.InvalidDepthRange,
                    $"{ValidationErrorType.InvalidDepthRange.GetDescription()} (DepthFrom: {row.DepthFrom}, DepthTo: {row.DepthTo})."
                    ));
            else if (row.Porosity < 0 || row.Porosity > 1)
                errors.Add(CreateError(
                    row, 
                    ValidationErrorType.InvalidPorosityValue,
                    $"{ValidationErrorType.InvalidPorosityValue.GetDescription()} (Porosity: {row.Porosity})."
                    ));
            else if (string.IsNullOrWhiteSpace(row.Rock))
                errors.Add(CreateError(
                    row, 
                    ValidationErrorType.MissingRockType,
                    $"{ValidationErrorType.MissingRockType.GetDescription()} (Rock: '{row.Rock}')."
                    ));
            else
                structurallyValidRows.Add(row); //good result
        }

        // check intervals for each well for intersection
        var groupedByWell = structurallyValidRows.GroupBy(r => r.WellId);

        foreach (var wellGroup in groupedByWell)
        {
            var sortedIntervals = wellGroup.OrderBy(r => r.DepthFrom).ToList();

            if (sortedIntervals.Count == 0) continue;

            validRows.Add(sortedIntervals[0]);
            double currentMaxDepthTo = sortedIntervals[0].DepthTo;

            for (int i = 1; i < sortedIntervals.Count; i++)
            {
                var current = sortedIntervals[i];

                if (current.DepthFrom < currentMaxDepthTo)
                {
                    errors.Add(CreateError(
                        current, 
                        ValidationErrorType.OverlappingIntervals,
                        $"{ValidationErrorType.OverlappingIntervals.GetDescription()} (DepthFrom: {current.DepthFrom}, overlaps with previous DepthTo: {currentMaxDepthTo})."
                        ));
                }
                else
                {
                    validRows.Add(current);
                    currentMaxDepthTo = Math.Max(currentMaxDepthTo, current.DepthTo);
                }
            }
        }

        return (validRows, errors);
    }

    private ValidationError CreateError(CsvRow row, ValidationErrorType errorType, string details) => new()
    {
        LineWithError = row.LineNumber,
        WellId = row.WellId,
        ErrorType = errorType,
        ErrorDetails = details,
        
    };
}
namespace AurumSoftTask.Core.Models
{
    public class ValidationError
    {
        public int LineWithError { get; init; }
        public string WellId { get; init; }
        public string ErrorDetails { get; init; }
        public ValidationErrorType ErrorType { get; init; }
    }

    public enum ValidationErrorType
    {
        MissingWellId,
        InvalidDepthRange,
        InvalidPorosityValue,
        MissingRockType,
        DuplicateWellId
    }
}

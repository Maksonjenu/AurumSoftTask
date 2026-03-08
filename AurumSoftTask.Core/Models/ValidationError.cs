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
        DuplicateWellId,
        IncorrectColumnCount,
        NumericFieldParseError
    }

    public static class ValidationErrorExtensions
    {
        public static string GetDescription(this ValidationErrorType type) => type switch
        {
            ValidationErrorType.MissingWellId => "Идентификатор скважины отсутствует.",
            ValidationErrorType.InvalidDepthRange => "DepthFrom должен быть меньше DepthTo.",
            ValidationErrorType.InvalidPorosityValue => "Пористость должна быть в диапазоне [0..1].",
            ValidationErrorType.MissingRockType => "Название породы не может быть пустым.",
            ValidationErrorType.DuplicateWellId => "Идентификатор скважины должен быть уникальным.",
            ValidationErrorType.IncorrectColumnCount => "Неверное количество столбцов в строке.",
            ValidationErrorType.NumericFieldParseError => "Ошибка при разборе числового поля.",
            _ => "Неизвестная ошибка."
        };
    }
}

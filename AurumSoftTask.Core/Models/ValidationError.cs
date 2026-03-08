namespace AurumSoftTask.Core.Models
{
    public class ValidationError
    {
        public int LineWithError { get; set; }
        public string WellId { get; set; }
        public string ErrorMessage { get; set; }
    }
}

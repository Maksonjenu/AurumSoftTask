namespace AurumSoftTask.Core.Models
{
    public record Interval
    {
        public double DepthFrom { get; init; }
        public double DepthTo { get; init; }
        public string Rock { get; init; }
        public double Porosity { get; init; }
        public int LineNumber { get; set; }

    }
}

namespace AurumSoftTask.Core.Models
{
    public record WellSummary

    {
        public string WellId { get; init; }
        public double TotalDepth { get; init; }
        public int IntervalCount { get; init; }
        public double AveragePorosity { get; init; }
        public string TopRockType { get; init; }
    }
}

namespace AurumSoftTask.Core.Models
{
    public record CsvRow
    {
        public CsvRow(int lineNumber, string wellId, double x, double y, double depthFrom, double depthTo, string rock, double porosity)
        {
            LineNumber = lineNumber;
            WellId = wellId;
            X = x;
            Y = y;
            DepthFrom = depthFrom;
            DepthTo = depthTo;
            Rock = rock;
            Porosity = porosity;
        }

        public int LineNumber { get; init; }
        public string WellId { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double DepthFrom { get; init; }
        public double DepthTo { get; init; }
        public string Rock { get; init; }
        public double Porosity { get; init; }
    }

}

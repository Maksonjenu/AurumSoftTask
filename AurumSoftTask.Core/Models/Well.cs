namespace AurumSoftTask.Core.Models
{
    public record Well
    {
        public string WellId { get; init; }
        public double X { get; set; }
        public double Y { get; set; }

        public List<Interval> Intervals { get; set; }

        public Well()
        {
            Intervals = new List<Interval>();
        }
    }
}

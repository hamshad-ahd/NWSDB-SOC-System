namespace NWSDB.Api.Models
{
    public class WaterUsage
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string MonthYear { get; set; } = string.Empty; // e.g. "2026-08"
        public int UnitsConsumed { get; set; }
        public DateTime ReadingDate { get; set; }
    }
}

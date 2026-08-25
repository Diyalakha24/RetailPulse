namespace RetailPulse.Models
{
    /// <summary>
    /// Simple filter criteria shared by the dashboard controller and the
    /// analytics service. Passing one small object around is simpler than
    /// passing four loose parameters through every method.
    
    public class SalesFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Category { get; set; }
        public string? Region { get; set; }
    }
}

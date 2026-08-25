namespace RetailPulse.Models.ViewModels
{
    /// <summary>
    /// The four headline KPI numbers shown at the top of the dashboard.
  
    public class KpiViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int UnitsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string TopProduct { get; set; } = "N/A";
    }
}

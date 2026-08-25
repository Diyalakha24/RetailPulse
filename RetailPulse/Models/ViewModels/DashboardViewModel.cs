namespace RetailPulse.Models.ViewModels
{
    /// <summary>
    /// Everything the dashboard view (and the JSON filter endpoint) needs to
    /// render KPIs, all four charts and the business insights in one shot.
   
    public class DashboardViewModel
    {
        public bool HasData { get; set; }

        public KpiViewModel Kpis { get; set; } = new();

        public List<ChartPointViewModel> RevenueOverTime { get; set; } = new();
        public List<ChartPointViewModel> RevenueByCategory { get; set; } = new();
        public List<ChartPointViewModel> TopProducts { get; set; } = new();
        public List<ChartPointViewModel> RevenueByRegion { get; set; } = new();

        public List<InsightViewModel> Insights { get; set; } = new();

        // Filter option lists, used to populate the dropdowns.
        public List<string> Categories { get; set; } = new();
        public List<string> Regions { get; set; } = new();

        // Currently applied filters, so the view can keep the controls in sync.
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedRegion { get; set; }
    }
}

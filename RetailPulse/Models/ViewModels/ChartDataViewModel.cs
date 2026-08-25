namespace RetailPulse.Models.ViewModels
{
    /// <summary>
    /// Generic label/value pair used to feed Chart.js. Keeping one simple shape
    /// for all four charts avoids having four bespoke DTOs for no real benefit.
    
    public class ChartPointViewModel
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}

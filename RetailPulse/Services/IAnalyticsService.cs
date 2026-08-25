using RetailPulse.Models;
using RetailPulse.Models.ViewModels;

namespace RetailPulse.Services
{
    
    // Calculates all KPI, chart and insight data for the dashboard from the
    // sales stored in the database. Kept separate from the controller so the
    // business logic can be unit tested without any ASP.NET Core / EF Core
    // plumbing involved (see RetailPulse.Tests).
   
    public interface IAnalyticsService
    {
        DashboardViewModel BuildDashboard(List<Sale> sales, SalesFilter filter);

        decimal CalculateTotalRevenue(List<Sale> sales);
        int CalculateUnitsSold(List<Sale> sales);
        decimal CalculateAverageOrderValue(List<Sale> sales);
        string GetTopProduct(List<Sale> sales);
        List<ChartPointViewModel> GetRevenueByCategory(List<Sale> sales);
        List<ChartPointViewModel> GetRevenueByRegion(List<Sale> sales);
        List<ChartPointViewModel> GetTopProducts(List<Sale> sales, int count);
        List<ChartPointViewModel> GetRevenueOverTime(List<Sale> sales);
        decimal? CalculateMonthlyGrowthPercentage(List<Sale> sales);
        List<InsightViewModel> GenerateInsights(List<Sale> sales);
    }
}

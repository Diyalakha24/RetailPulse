using RetailPulse.Models;
using RetailPulse.Models.ViewModels;

namespace RetailPulse.Services
{
    /// <summary>
    /// Pure business-logic service that turns a list of Sale records into the
    /// KPIs, chart data and automated insights shown on the dashboard.
    ///
    /// Every method takes the already-loaded (and, where relevant, filtered)
    /// list of sales as a plain in-memory collection rather than an
    /// IQueryable/DbContext. That keeps the class free of any database or
    /// ASP.NET Core dependency, which is what makes it straightforward to
    /// unit test (see RetailPulse.Tests/AnalyticsServiceTests.cs).
    
    public class AnalyticsService : IAnalyticsService
    {
        public DashboardViewModel BuildDashboard(List<Sale> sales, SalesFilter filter)
        {
            var viewModel = new DashboardViewModel
            {
                HasData = sales.Count > 0,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                SelectedCategory = filter.Category,
                SelectedRegion = filter.Region
            };

            if (!viewModel.HasData)
            {
                return viewModel;
            }

            viewModel.Kpis = new KpiViewModel
            {
                TotalRevenue = CalculateTotalRevenue(sales),
                UnitsSold = CalculateUnitsSold(sales),
                AverageOrderValue = CalculateAverageOrderValue(sales),
                TopProduct = GetTopProduct(sales)
            };

            viewModel.RevenueOverTime = GetRevenueOverTime(sales);
            viewModel.RevenueByCategory = GetRevenueByCategory(sales);
            viewModel.TopProducts = GetTopProducts(sales, 5);
            viewModel.RevenueByRegion = GetRevenueByRegion(sales);
            viewModel.Insights = GenerateInsights(sales);

            return viewModel;
        }

        //  Descriptive analytics: "what happened?" 

        public decimal CalculateTotalRevenue(List<Sale> sales) => sales.Sum(s => s.Revenue);

        public int CalculateUnitsSold(List<Sale> sales) => sales.Sum(s => s.Quantity);

        public decimal CalculateAverageOrderValue(List<Sale> sales)
        {
            if (sales.Count == 0) return 0m;
            return CalculateTotalRevenue(sales) / sales.Count;
        }

        // Diagnostic-style analysis: "what performed best?" 

        public string GetTopProduct(List<Sale> sales)
        {
            if (sales.Count == 0) return "N/A";

            return sales
                .GroupBy(s => s.Product)
                .Select(g => new { Product = g.Key, Revenue = g.Sum(s => s.Revenue) })
                .OrderByDescending(x => x.Revenue)
                .First()
                .Product;
        }

        public List<ChartPointViewModel> GetRevenueByCategory(List<Sale> sales)
        {
            return sales
                .GroupBy(s => s.Category)
                .Select(g => new ChartPointViewModel { Label = g.Key, Value = g.Sum(s => s.Revenue) })
                .OrderByDescending(c => c.Value)
                .ToList();
        }

        public List<ChartPointViewModel> GetRevenueByRegion(List<Sale> sales)
        {
            return sales
                .GroupBy(s => s.Region)
                .Select(g => new ChartPointViewModel { Label = g.Key, Value = g.Sum(s => s.Revenue) })
                .OrderByDescending(c => c.Value)
                .ToList();
        }

        public List<ChartPointViewModel> GetTopProducts(List<Sale> sales, int count)
        {
            return sales
                .GroupBy(s => s.Product)
                .Select(g => new ChartPointViewModel { Label = g.Key, Value = g.Sum(s => s.Revenue) })
                .OrderByDescending(c => c.Value)
                .Take(count)
                .ToList();
        }

        // Trend analysis: "how is performance changing?" 

        public List<ChartPointViewModel> GetRevenueOverTime(List<Sale> sales)
        {
            return sales
                .GroupBy(s => new DateTime(s.Date.Year, s.Date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new ChartPointViewModel
                {
                    Label = g.Key.ToString("MMM yyyy"),
                    Value = g.Sum(s => s.Revenue)
                })
                .ToList();
        }

        /// <summary>
        /// Compares total revenue for the most recent month present in the
        /// data against the month immediately before it. Returns null when
        /// there is not enough historical data (fewer than two distinct
        /// months) to make a comparison.
        
        public decimal? CalculateMonthlyGrowthPercentage(List<Sale> sales)
        {
            var monthlyRevenue = sales
                .GroupBy(s => new DateTime(s.Date.Year, s.Date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(s => s.Revenue) })
                .ToList();

            if (monthlyRevenue.Count < 2)
            {
                return null;
            }

            var currentMonth = monthlyRevenue[^1];
            var previousMonth = monthlyRevenue[^2];

            if (previousMonth.Revenue == 0)
            {
                return null;
            }

            var change = (currentMonth.Revenue - previousMonth.Revenue) / previousMonth.Revenue * 100m;
            return Math.Round(change, 1);
        }

        // Business insights: "what should the user notice?" 

        public List<InsightViewModel> GenerateInsights(List<Sale> sales)
        {
            var insights = new List<InsightViewModel>();

            if (sales.Count == 0)
            {
                return insights;
            }

            var totalRevenue = CalculateTotalRevenue(sales);

            // Insight: best-performing category.
            var categoryRevenue = GetRevenueByCategory(sales);
            if (categoryRevenue.Count > 0)
            {
                var topCategory = categoryRevenue.First();
                insights.Add(new InsightViewModel
                {
                    Text = $"{topCategory.Label} generated the highest revenue across all categories.",
                    Direction = InsightDirection.Positive
                });

                if (totalRevenue > 0)
                {
                    var share = Math.Round(topCategory.Value / totalRevenue * 100m, 0);
                    insights.Add(new InsightViewModel
                    {
                        Text = $"{topCategory.Label} accounted for {share}% of total revenue.",
                        Direction = InsightDirection.Neutral
                    });
                }
            }

            // Insight: top product.
            var topProduct = GetTopProduct(sales);
            if (topProduct != "N/A")
            {
                insights.Add(new InsightViewModel
                {
                    Text = $"{topProduct} was the highest-revenue product.",
                    Direction = InsightDirection.Positive
                });
            }

            // Insight: best-performing region.
            var regionRevenue = GetRevenueByRegion(sales);
            if (regionRevenue.Count > 0)
            {
                var topRegion = regionRevenue.First();
                insights.Add(new InsightViewModel
                {
                    Text = $"{topRegion.Label} generated the highest regional revenue.",
                    Direction = InsightDirection.Positive
                });
            }

            // Insight: month-over-month trend.
            var growth = CalculateMonthlyGrowthPercentage(sales);
            if (growth == null)
            {
                insights.Add(new InsightViewModel
                {
                    Text = "Not enough historical data to calculate a monthly comparison.",
                    Direction = InsightDirection.Neutral
                });
            }
            else if (growth > 0)
            {
                insights.Add(new InsightViewModel
                {
                    Text = $"Revenue increased {growth}% compared with the previous month.",
                    Direction = InsightDirection.Positive
                });
            }
            else if (growth < 0)
            {
                insights.Add(new InsightViewModel
                {
                    Text = $"Revenue decreased {Math.Abs(growth.Value)}% compared with the previous month.",
                    Direction = InsightDirection.Negative
                });
            }
            else
            {
                insights.Add(new InsightViewModel
                {
                    Text = "Revenue was unchanged compared with the previous month.",
                    Direction = InsightDirection.Neutral
                });
            }

            return insights;
        }
    }
}

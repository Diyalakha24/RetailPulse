using RetailPulse.Models;
using RetailPulse.Services;
using Xunit;

namespace RetailPulse.Tests
{
    /// <summary>
    /// Unit tests for AnalyticsService. These test the actual business rules
    /// (revenue totals, averages, rankings, growth percentages) rather than
    /// trivial getters/setters, using small hand-built in-memory datasets so
    /// each expected result can be verified by hand.
    /// </summary>
    public class AnalyticsServiceTests
    {
        private readonly AnalyticsService _service = new();

        private static List<Sale> BuildSampleSales()
        {
            return new List<Sale>
            {
                new() { TransactionId = 1, Date = new DateTime(2026, 1, 10), Product = "Laptop", Category = "Electronics", Region = "Gauteng", Quantity = 1, UnitPrice = 10000, Revenue = 10000 },
                new() { TransactionId = 2, Date = new DateTime(2026, 1, 15), Product = "Mouse", Category = "Accessories", Region = "Western Cape", Quantity = 2, UnitPrice = 200, Revenue = 400 },
                new() { TransactionId = 3, Date = new DateTime(2026, 2, 5), Product = "Laptop", Category = "Electronics", Region = "Gauteng", Quantity = 1, UnitPrice = 12000, Revenue = 12000 },
                new() { TransactionId = 4, Date = new DateTime(2026, 2, 20), Product = "Office Chair", Category = "Furniture", Region = "KwaZulu-Natal", Quantity = 3, UnitPrice = 1000, Revenue = 3000 },
            };
        }

        [Fact]
        public void CalculateTotalRevenue_SumsRevenueAcrossAllSales()
        {
            var sales = BuildSampleSales();

            var total = _service.CalculateTotalRevenue(sales);

            Assert.Equal(25400m, total);
        }

        [Fact]
        public void CalculateAverageOrderValue_DividesTotalRevenueByTransactionCount()
        {
            var sales = BuildSampleSales();

            var average = _service.CalculateAverageOrderValue(sales);

            // 25400 total revenue / 4 transactions = 6350
            Assert.Equal(6350m, average);
        }

        [Fact]
        public void CalculateAverageOrderValue_ReturnsZero_WhenNoSales()
        {
            var average = _service.CalculateAverageOrderValue(new List<Sale>());

            Assert.Equal(0m, average);
        }

        [Fact]
        public void GetTopProduct_ReturnsProductWithHighestTotalRevenue()
        {
            var sales = BuildSampleSales();

            var topProduct = _service.GetTopProduct(sales);

            // Laptop: 10000 + 12000 = 22000, the highest of any product.
            Assert.Equal("Laptop", topProduct);
        }

        [Fact]
        public void GetRevenueByCategory_GroupsAndSumsRevenuePerCategory_OrderedDescending()
        {
            var sales = BuildSampleSales();

            var result = _service.GetRevenueByCategory(sales);

            Assert.Equal(3, result.Count);
            Assert.Equal("Electronics", result[0].Label);
            Assert.Equal(22000m, result[0].Value);
            Assert.Equal("Furniture", result[1].Label);
            Assert.Equal(3000m, result[1].Value);
            Assert.Equal("Accessories", result[2].Label);
            Assert.Equal(400m, result[2].Value);
        }

        [Fact]
        public void CalculateMonthlyGrowthPercentage_ComparesLastTwoMonths()
        {
            var sales = BuildSampleSales();

            // January revenue: 10400, February revenue: 15000
            // Growth = (15000 - 10400) / 10400 * 100 = 44.2%
            var growth = _service.CalculateMonthlyGrowthPercentage(sales);

            Assert.NotNull(growth);
            Assert.Equal(44.2m, growth);
        }

        [Fact]
        public void CalculateMonthlyGrowthPercentage_ReturnsNull_WhenOnlyOneMonthOfData()
        {
            var sales = new List<Sale>
            {
                new() { TransactionId = 1, Date = new DateTime(2026, 1, 10), Product = "Laptop", Category = "Electronics", Region = "Gauteng", Quantity = 1, UnitPrice = 10000, Revenue = 10000 }
            };

            var growth = _service.CalculateMonthlyGrowthPercentage(sales);

            Assert.Null(growth);
        }
    }
}

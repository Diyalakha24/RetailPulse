using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Data;
using RetailPulse.Models;
using RetailPulse.Services;

namespace RetailPulse.Controllers
{
    /// <summary>
    /// Serves the main analytics dashboard. The controller's job is limited to
    /// loading data and applying filters - all KPI/chart/insight calculation
    /// is delegated to IAnalyticsService, keeping the controller thin.
   
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAnalyticsService _analyticsService;

        public DashboardController(ApplicationDbContext context, IAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        // GET: /Dashboard  
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? category, string? region)
        {
            var filter = new SalesFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                Category = string.IsNullOrWhiteSpace(category) || category == "All Categories" ? null : category,
                Region = string.IsNullOrWhiteSpace(region) || region == "All Regions" ? null : region
            };

            var sales = await GetFilteredSalesAsync(filter);
            var viewModel = _analyticsService.BuildDashboard(sales, filter);

            // Filter dropdowns always list every category/region that exists in
            // the database, not just the ones present in the filtered result -
            // otherwise a filtered-out option would disappear from its own dropdown.
            viewModel.Categories = await _context.Sales.Select(s => s.Category).Distinct().OrderBy(c => c).ToListAsync();
            viewModel.Regions = await _context.Sales.Select(s => s.Region).Distinct().OrderBy(r => r).ToListAsync();

            return View(viewModel);
        }

        /// <summary>
        /// JSON endpoint used by the dashboard's JavaScript to refresh KPIs,
        /// charts and insights when a filter changes, without a full page reload.
        
        [HttpGet]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate, string? category, string? region)
        {
            var filter = new SalesFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                Category = string.IsNullOrWhiteSpace(category) || category == "All Categories" ? null : category,
                Region = string.IsNullOrWhiteSpace(region) || region == "All Regions" ? null : region
            };

            var sales = await GetFilteredSalesAsync(filter);
            var viewModel = _analyticsService.BuildDashboard(sales, filter);

            return Json(viewModel);
        }

        /// <summary>
        /// Generic friendly error page. Never shows exception details - see the
        /// project README for the "never expose stack traces" requirement.
        
        public IActionResult Error()
        {
            return View();
        }

        private async Task<List<Sale>> GetFilteredSalesAsync(SalesFilter filter)
        {
            var query = _context.Sales.AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(s => s.Date >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(s => s.Date <= filter.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(s => s.Category == filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.Region))
            {
                query = query.Where(s => s.Region == filter.Region);
            }

            return await query.OrderBy(s => s.Date).ToListAsync();
        }
    }
}

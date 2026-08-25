using Microsoft.AspNetCore.Mvc;
using RetailPulse.Models.ViewModels;
using RetailPulse.Services;

namespace RetailPulse.Controllers
{
    /// <summary>
    /// Handles the "Import Sales Data" page. All parsing, validation,
    /// cleaning and duplicate handling lives in ICsvImportService - this
    /// controller only accepts the upload and displays the result.
    
    public class ImportController : Controller
    {
        private readonly ICsvImportService _csvImportService;

        public ImportController(ICsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        // GET: /Import
        public IActionResult Index()
        {
            return View(new ImportResultViewModel());
        }

        // POST: /Import
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10 MB is more than enough for a CSV of a few thousand rows.
        public async Task<IActionResult> Index(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return View(new ImportResultViewModel
                {
                    Success = false,
                    Message = "Please choose a CSV file to import."
                });
            }

            ImportResultViewModel result;

            try
            {
                using var stream = file.OpenReadStream();
                result = await _csvImportService.ImportAsync(stream, file.FileName);
            }
            catch
            {
                // Defensive fallback - CsvImportService already catches its own
                // exceptions, but no user-facing exception detail ever leaks.
                result = new ImportResultViewModel
                {
                    Success = false,
                    Message = "Unable to import the file. Please ensure it contains the required sales columns."
                };
            }

            return View(result);
        }
    }
}

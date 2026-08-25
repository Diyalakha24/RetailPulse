using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Data;
using RetailPulse.Models;
using RetailPulse.Models.ViewModels;

namespace RetailPulse.Services
{
    
    /// Reads an uploaded CSV file, validates and cleans every row, skips
    /// duplicate transactions and stores the remaining valid rows in SQLite.
    ///
    /// The service works with raw strings from the CSV rather than letting
    /// CsvHelper auto-map straight onto the Sale entity. That is deliberate:
    /// auto-mapping throws hard-to-control exceptions on bad data, whereas
    /// reading raw fields lets us validate every value ourselves and simply
    // skip a bad row instead of crashing the whole import.
    
    public class CsvImportService : ICsvImportService
    {
        private static readonly string[] RequiredColumns =
        {
            "TransactionId", "Date", "Product", "Category", "Region", "Quantity", "UnitPrice", "Revenue"
        };

        private readonly ApplicationDbContext _context;

        public CsvImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResultViewModel> ImportAsync(Stream csvFileStream, string fileName)
        {
            if (csvFileStream == null || csvFileStream.Length == 0)
            {
                return Failure("Unable to import the file. Please ensure it contains the required sales columns.");
            }

            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Failure("Unable to import the file. Please upload a .csv file.");
            }

            try
            {
                using var reader = new StreamReader(csvFileStream);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim
                };
                using var csv = new CsvReader(reader, config);

                if (!await csv.ReadAsync() || !csv.ReadHeader())
                {
                    return Failure("Unable to import the file. Please ensure it contains the required sales columns.");
                }

                var headers = csv.HeaderRecord ?? Array.Empty<string>();
                var missingColumns = RequiredColumns.Where(c => !headers.Contains(c, StringComparer.OrdinalIgnoreCase));
                if (missingColumns.Any())
                {
                    return Failure("Unable to import the file. Please ensure it contains the required sales columns.");
                }

                // Load existing transaction IDs once, so duplicate checking is an
                // in-memory lookup rather than a database round trip per row.
                var existingTransactionIds = (await _context.Sales
                        .Select(s => s.TransactionId)
                        .ToListAsync())
                    .ToHashSet();

                var newSales = new List<Sale>();
                var seenInThisFile = new HashSet<int>();
                int duplicatesSkipped = 0;
                int invalidRowsSkipped = 0;

                while (await csv.ReadAsync())
                {
                    var row = TryParseRow(csv);

                    if (row == null)
                    {
                        invalidRowsSkipped++;
                        continue;
                    }

                    if (existingTransactionIds.Contains(row.TransactionId) || seenInThisFile.Contains(row.TransactionId))
                    {
                        duplicatesSkipped++;
                        continue;
                    }

                    seenInThisFile.Add(row.TransactionId);
                    newSales.Add(row);
                }

                if (newSales.Count > 0)
                {
                    await _context.Sales.AddRangeAsync(newSales);
                    await _context.SaveChangesAsync();
                }

                return BuildSuccessResult(newSales.Count, duplicatesSkipped, invalidRowsSkipped);
            }
            catch
            {
                // Any unexpected parsing/IO failure is surfaced as a friendly
                // message only - the real exception is never shown to the user.
                return Failure("Unable to import the file. Please ensure it contains the required sales columns.");
            }
        }

       
        // Parses and validates a single CSV row. Returns null if the row
        // should be skipped (missing, malformed or otherwise invalid data).
        // This is the "data cleaning" step described in the README:
        // trimming whitespace, checking numeric/date formats and rejecting
        // business-invalid values such as negative revenue or zero quantity.
        
        private static Sale? TryParseRow(CsvReader csv)
        {
            string? transactionIdRaw = csv.GetField("TransactionId")?.Trim();
            string? dateRaw = csv.GetField("Date")?.Trim();
            string? product = csv.GetField("Product")?.Trim();
            string? category = csv.GetField("Category")?.Trim();
            string? region = csv.GetField("Region")?.Trim();
            string? quantityRaw = csv.GetField("Quantity")?.Trim();
            string? unitPriceRaw = csv.GetField("UnitPrice")?.Trim();
            string? revenueRaw = csv.GetField("Revenue")?.Trim();

            if (string.IsNullOrWhiteSpace(transactionIdRaw) ||
                string.IsNullOrWhiteSpace(dateRaw) ||
                string.IsNullOrWhiteSpace(product) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(quantityRaw) ||
                string.IsNullOrWhiteSpace(unitPriceRaw) ||
                string.IsNullOrWhiteSpace(revenueRaw))
            {
                return null;
            }

            if (!int.TryParse(transactionIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var transactionId))
            {
                return null;
            }

            if (!DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return null;
            }

            if (!int.TryParse(quantityRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
            {
                return null;
            }

            if (!decimal.TryParse(unitPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var unitPrice) || unitPrice < 0)
            {
                return null;
            }

            if (!decimal.TryParse(revenueRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var revenue) || revenue < 0)
            {
                return null;
            }

            return new Sale
            {
                TransactionId = transactionId,
                Date = date,
                Product = product,
                Category = category,
                Region = region,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Revenue = revenue
            };
        }

        private static ImportResultViewModel Failure(string message) => new()
        {
            Success = false,
            Message = message
        };

        private static ImportResultViewModel BuildSuccessResult(int imported, int duplicates, int invalid)
        {
            if (imported == 0 && duplicates == 0 && invalid == 0)
            {
                return Failure("The file did not contain any sales records to import.");
            }

            if (imported == 0)
            {
                return Failure("Unable to import the file. No valid sales records were found.");
            }

            var message = $"{imported} record{(imported == 1 ? "" : "s")} imported successfully.";

            if (duplicates > 0)
            {
                message += $" {duplicates} duplicate record{(duplicates == 1 ? "" : "s")} skipped.";
            }

            if (invalid > 0)
            {
                message += $" {invalid} invalid record{(invalid == 1 ? "" : "s")} skipped.";
            }

            return new ImportResultViewModel
            {
                Success = true,
                Message = message,
                RecordsImported = imported,
                DuplicatesSkipped = duplicates,
                InvalidRowsSkipped = invalid
            };
        }
    }
}

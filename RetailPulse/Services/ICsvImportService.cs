using RetailPulse.Models.ViewModels;

namespace RetailPulse.Services
{
   
    // Handles validating, cleaning and importing a CSV file of sales
    // transactions into the database.
    
    public interface ICsvImportService
    {
        Task<ImportResultViewModel> ImportAsync(Stream csvFileStream, string fileName);
    }
}

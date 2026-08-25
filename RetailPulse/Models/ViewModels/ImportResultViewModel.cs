namespace RetailPulse.Models.ViewModels
{
    /// <summary>
    /// Result returned by the CSV import service, used to build the
    /// success/error message shown on the Import page.
   
    public class ImportResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public int RecordsImported { get; set; }
        public int DuplicatesSkipped { get; set; }
        public int InvalidRowsSkipped { get; set; }
    }
}

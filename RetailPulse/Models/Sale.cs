using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Models
{
    /// <summary>
    /// Represents a single retail sales transaction imported from a CSV file.
    /// This is the only persisted entity in the project - keeping a single table
    /// is intentional, since the goal of RetailPulse is analytics, not full
    /// retail management (no customers, no inventory, no orders workflow).
   
    public class Sale
    {
        public int Id { get; set; }

        /// <summary>
        /// The transaction identifier as it appears in the source CSV file.
        /// Used to detect and skip duplicate imports.
      
        [Required]
        public int TransactionId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(100)]
        public string Product { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Region { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal Revenue { get; set; }
    }
}

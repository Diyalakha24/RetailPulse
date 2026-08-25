using Microsoft.EntityFrameworkCore;
using RetailPulse.Models;

namespace RetailPulse.Data
{
    /// <summary>
    /// EF Core database context for RetailPulse.
    /// SQLite is used because it needs no separate database server - the whole
    /// database is a single file, which keeps the project trivial to clone,
    /// run and demo (important for a portfolio project a recruiter might run).
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sale> Sales => Set<Sale>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sale>(entity =>
            {
                // A unique index on TransactionId is what makes duplicate-import
                // detection cheap and reliable at the database level.
                entity.HasIndex(s => s.TransactionId).IsUnique();

                entity.Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Revenue).HasColumnType("decimal(18,2)");
            });
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KLENZ.Models;

namespace KLENZ.Data // ✅ Correct namespace
{
    public class KLENZDbContext : IdentityDbContext<IdentityUser>
    {
        public KLENZDbContext(DbContextOptions<KLENZDbContext> options) : base(options)
        {
        }

        public DbSet<SalesEnquiry> SalesEnquiries { get; set; } = default!;
        public DbSet<Status> Status { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SalesEnquiry>().ToTable("SalesEnquiry", "Sales"); // Ensure schema
            modelBuilder.Entity<PositiveEnquiry>()
            .Property(p => p.QuotationValue)
            .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<ProjectList>()
                  .Property(p => p.WorkOrderValue)
                  .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<QuotationReport>()
                .Property(q => q.QuotationValue)
                .HasColumnType("decimal(18,2)");
        }
        public DbSet<KLENZ.Models.QuotationReport> QuotationReport { get; set; } = default!;
        public DbSet<KLENZ.Models.PositiveEnquiry> PositiveEnquiry { get; set; } = default!;
        public DbSet<KLENZ.Models.FinancialYear> FinancialYear { get; set; } = default!;
        public DbSet<KLENZ.Models.ProjectList> ProjectList { get; set; } = default!;
    }
}

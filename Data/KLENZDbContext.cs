using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KLENZ.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace KLENZ.Data // ✅ Correct namespace
{
    public class KLENZDbContext : IdentityDbContext<ApplicationUser>
    {
        public KLENZDbContext(DbContextOptions<KLENZDbContext> options) : base(options)
        {
        }

        public DbSet<SalesEnquiry> SalesEnquiries { get; set; } = default!;
        public DbSet<Status> Status { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exclude non-Identity tables from EF Core migrations as they are managed via SQL files
            modelBuilder.Entity<SalesEnquiry>().ToTable("SalesEnquiry", "Sales", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Status>().ToTable("Status", "Sales", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<CompanyName>().ToTable("Companies", "Services", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<PositiveEnquiry>().ToTable("PositiveEnquiry", "Sales", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<ProjectList>().ToTable("ProjectList", "Sales", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<QuotationReport>().ToTable("QuotationReport", "Sales", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<FinancialYear>().ToTable("FinancialYear", "Services", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<GSTTypes>().ToTable("GSTTypes", "Services", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Project_Consultancy>().ToTable("Consultancy", "Project", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Project_TenderList>().ToTable("TenderList", "Project", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Project_ToBeTenderList>().ToTable("ToBeTenderList", "Project", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Project_KlenzChemicals>().ToTable("KlenzChemicals", "Project", t => t.ExcludeFromMigrations());

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
        public DbSet<KLENZ.Models.CompanyName> CompanyName { get; set; } = default!;
        public DbSet<KLENZ.Models.GSTTypes> GSTTypes { get; set; } = default!;
        public DbSet<KLENZ.Models.Project_Consultancy> Project_Consultancy { get; set; } = default!;
        public DbSet<KLENZ.Models.Project_TenderList> Project_TenderList { get; set; } = default!;
        public DbSet<KLENZ.Models.Project_ToBeTenderList> Project_ToBeTenderList { get; set; } = default!;
        public DbSet<KLENZ.Models.Project_KlenzChemicals> Project_KlenzChemicals { get; set; } = default!;
    }
}

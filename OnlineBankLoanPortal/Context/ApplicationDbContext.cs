using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Models;

namespace OnlineBankLoanPortal.Repository;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LoanApplication> LoanApplications { get; set; }

    //public virtual DbSet<LoanProduct> LoanProducts { get; set; }

    public virtual DbSet<Repayment> Repayments { get; set; }

    public DbSet<LoanType> LoanTypes { get; set; }
    public DbSet<InterestRate> InterestRates { get; set; }
    public DbSet<RepaymentTerm> RepaymentTerms { get; set; }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<LoanApplicationDocument> LoanApplicationDocuments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Initial Catalog=LoanPortalDb2;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.LoanApplicationId).HasName("PK__LoanAppl__C93A4F79D4D81F6D");

            entity.Property(e => e.AmountRequested).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DateApplied)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.Feedback).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Product).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__LoanAppli__Produ__29572725");

            entity.HasOne(d => d.Customer).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_LoanApplications_AspNetUsers");
        });

        modelBuilder.Entity<LoanProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__LoanProd__B40CC6ED2A47D192");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.RepaymentTerms).HasMaxLength(100);
        });

        modelBuilder.Entity<Repayment>(entity =>
        {
            entity.HasKey(e => e.RepaymentId).HasName("PK__Repaymen__10AD21D2B4A22E4A");

            entity.Property(e => e.RepaymentId).HasColumnName("RepaymentID");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReceiptUpload).HasMaxLength(256);

            entity.HasOne(d => d.Application).WithMany(p => p.Repayments)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK__Repayment__Appli__2D27B809");
        });

        modelBuilder.Entity<LoanProduct>().HasData(
            new LoanProduct
            {
                ProductId = 1,
                ProductName = "Personal",
                InterestRate = 5.0m,
                RepaymentTerms = "1-5 years"
            },
            new LoanProduct
            {
                ProductId = 2,
                ProductName = "Home",
                InterestRate = 3.5m,
                RepaymentTerms = "15-30 years"
            },
            new LoanProduct
            {
                ProductId = 3,
                ProductName = "Business",
                InterestRate = 6.0m,
                RepaymentTerms = "1-10 years"
            }
        );

        // Seed Loan Types
        modelBuilder.Entity<LoanType>().HasData(
            new LoanType { LoanTypeId = 1, LoanTypeName = "Personal Loan", Description = "Loan for personal expenses", IsActive = true, CreatedDate = DateTime.UtcNow },
            new LoanType { LoanTypeId = 2, LoanTypeName = "Home Loan", Description = "Loan for purchasing a home", IsActive = true, CreatedDate = DateTime.UtcNow },
            new LoanType { LoanTypeId = 3, LoanTypeName = "Auto Loan", Description = "Loan for purchasing a vehicle", IsActive = true, CreatedDate = DateTime.UtcNow }
        );

        // Seed Interest Rates
        modelBuilder.Entity<InterestRate>().HasData(
            new InterestRate { InterestRateId = 1, LoanTypeId = 1, Rate = 10.5M, EffectiveDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddYears(1), CreatedDate = DateTime.UtcNow },
            new InterestRate { InterestRateId = 2, LoanTypeId = 2, Rate = 8.0M, EffectiveDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddYears(5), CreatedDate = DateTime.UtcNow },
            new InterestRate { InterestRateId = 3, LoanTypeId = 3, Rate = 9.0M, EffectiveDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddYears(3), CreatedDate = DateTime.UtcNow }
        );

        // Seed Repayment Terms
        modelBuilder.Entity<RepaymentTerm>().HasData(
            new RepaymentTerm { RepaymentTermId = 1, LoanTypeId = 1, TermInMonths = 12, Description = "12-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow },
            new RepaymentTerm { RepaymentTermId = 2, LoanTypeId = 1, TermInMonths = 24, Description = "24-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow },
            new RepaymentTerm { RepaymentTermId = 3, LoanTypeId = 2, TermInMonths = 60, Description = "60-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow },
            new RepaymentTerm { RepaymentTermId = 4, LoanTypeId = 2, TermInMonths = 120, Description = "120-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow },
            new RepaymentTerm { RepaymentTermId = 5, LoanTypeId = 3, TermInMonths = 36, Description = "36-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow },
            new RepaymentTerm { RepaymentTermId = 6, LoanTypeId = 3, TermInMonths = 48, Description = "48-month repayment term", IsActive = true, CreatedDate = DateTime.UtcNow }
        );

        // Configure relationships and constraints here if needed
        modelBuilder.Entity<InterestRate>()
            .HasOne(ir => ir.LoanType)
            .WithMany(lt => lt.InterestRates)
            .HasForeignKey(ir => ir.LoanTypeId)
            .OnDelete(DeleteBehavior.Cascade); // Example: Delete interest rates when the loan type is deleted

        modelBuilder.Entity<RepaymentTerm>()
            .HasOne(rt => rt.LoanType)
            .WithMany(lt => lt.RepaymentTerms)
            .HasForeignKey(rt => rt.LoanTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

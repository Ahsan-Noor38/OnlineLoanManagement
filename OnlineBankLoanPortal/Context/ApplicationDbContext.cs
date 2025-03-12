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

    public virtual DbSet<LoanProduct> LoanProducts { get; set; }

    public virtual DbSet<Repayment> Repayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Initial Catalog=LoanPortalDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__LoanAppl__C93A4F79D4D81F6D");

            entity.Property(e => e.AmountRequested).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DateApplied)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateApproved).HasColumnType("datetime");
            entity.Property(e => e.Feedback).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Product).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__LoanAppli__Produ__29572725");

            entity.HasOne(d => d.ApplicationUser).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.UserId)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

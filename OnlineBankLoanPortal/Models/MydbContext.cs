using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OnlineBankLoanPortal.Models;

public partial class MydbContext : DbContext
{
    public MydbContext()
    {
    }

    public MydbContext(DbContextOptions<MydbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<LoanApplication> LoanApplications { get; set; }

    public virtual DbSet<LoanOfficer> LoanOfficers { get; set; }

    public virtual DbSet<LoanProduct> LoanProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CId);

            entity.ToTable("customer");

            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(225)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.LaId);

            entity.ToTable("loanApplication");

            entity.Property(e => e.LaId).HasColumnName("laId");
            entity.Property(e => e.Address)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.Amount)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("amount");
            entity.Property(e => e.ApplicantEmail)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.ApplicantName)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.Date)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("date");
            entity.Property(e => e.LoId).HasColumnName("loId");
            entity.Property(e => e.LoanProduct)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.LpId).HasColumnName("lpId");
            entity.Property(e => e.TotalIncome)
                .HasMaxLength(225)
                .IsUnicode(false);

            entity.HasOne(d => d.Lo).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.LoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_loanApplication_loanOfficer1");
        });

        modelBuilder.Entity<LoanOfficer>(entity =>
        {
            entity.HasKey(e => e.LoId);

            entity.ToTable("loanOfficer");

            entity.Property(e => e.LoId).HasColumnName("loId");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(225)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LoanProduct>(entity =>
        {
            entity.HasKey(e => e.LpId);

            entity.ToTable("loanProduct");

            entity.Property(e => e.LpId).HasColumnName("lpId");
            entity.Property(e => e.Name)
                .HasMaxLength(225)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

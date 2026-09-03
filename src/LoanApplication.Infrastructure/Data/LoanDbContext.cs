using LoanApplication.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Data;

public class LoanDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Application> Applications { get; set; }

    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Ssn).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.State).IsRequired().HasMaxLength(2);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Ssn).IsRequired().HasMaxLength(11);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestedAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
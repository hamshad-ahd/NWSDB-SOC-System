using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Models;

namespace NWSDB.Api.Data
{
    public class NwsdbDbContext : DbContext
    {
        public NwsdbDbContext(DbContextOptions<NwsdbDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ThirdPartyCollection> ThirdPartyCollections => Set<ThirdPartyCollection>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer Configuration & Constraints
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.AccountNumber).IsUnique();
                entity.Property(c => c.AccountNumber).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Address).IsRequired().HasMaxLength(250);
                entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
            });

            // Bill Configuration, Constraints & Indexes
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasIndex(b => b.BillNumber).IsUnique();
                entity.HasIndex(b => new { b.CustomerId, b.DueDate });
                entity.Property(b => b.BillNumber).IsRequired().HasMaxLength(50);
                entity.Property(b => b.BillingMonth).IsRequired().HasMaxLength(50);
                entity.Property(b => b.Status).IsRequired().HasMaxLength(30);
                entity.Property(b => b.BillAmount).HasColumnType("decimal(18,2)");
                entity.Property(b => b.PaidAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(b => b.Customer)
                      .WithMany(c => c.Bills)
                      .HasForeignKey(b => b.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment Configuration, Constraints & Indexes
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.ReceiptNumber).IsUnique();
                entity.HasIndex(p => p.TransactionId);
                entity.HasIndex(p => new { p.CustomerId, p.BillId });
                entity.Property(p => p.ReceiptNumber).IsRequired().HasMaxLength(50);
                entity.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(30);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(30);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Customer)
                      .WithMany(c => c.Payments)
                      .HasForeignKey(p => p.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Bill)
                      .WithMany(b => b.Payments)
                      .HasForeignKey(p => p.BillId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ThirdPartyCollection Configuration, Constraints & Indexes
            modelBuilder.Entity<ThirdPartyCollection>(entity =>
            {
                entity.HasIndex(t => new { t.CustomerId, t.BillId });
                entity.Property(t => t.PaymentMethod).IsRequired().HasMaxLength(30);
                entity.Property(t => t.CollectionStatus).IsRequired().HasMaxLength(30);
                entity.Property(t => t.TransferStatus).IsRequired().HasMaxLength(30);
                entity.Property(t => t.TransferReference).HasMaxLength(100);
                entity.Property(t => t.BankTransactionId).HasMaxLength(100);
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.Customer)
                      .WithMany(c => c.ThirdPartyCollections)
                      .HasForeignKey(t => t.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Bill)
                      .WithMany(b => b.ThirdPartyCollections)
                      .HasForeignKey(t => t.BillId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Bank.Api.Models;

namespace Bank.Api.Data
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasIndex(a => a.CardNumber).IsUnique();
                entity.Property(a => a.CardNumber).IsRequired().HasMaxLength(50);
                entity.Property(a => a.CardHolderName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.ExpiryDate).IsRequired().HasMaxLength(10);
                entity.Property(a => a.CVV).IsRequired().HasMaxLength(10);
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasIndex(t => t.ReferenceNumber).IsUnique();
                entity.HasIndex(t => t.BankAccountId);
                entity.Property(t => t.ReferenceNumber).IsRequired().HasMaxLength(100);
                entity.Property(t => t.TransactionType).IsRequired().HasMaxLength(30);
                entity.Property(t => t.Status).IsRequired().HasMaxLength(30);
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.BankAccount)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.BankAccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

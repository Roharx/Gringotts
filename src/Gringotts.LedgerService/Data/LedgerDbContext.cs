using Microsoft.EntityFrameworkCore;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.LedgerService;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using Gringotts.Shared.Models.LedgerService.UserService;

namespace Gringotts.LedgerService.Data
{
    public class LedgerDbContext : DbContext
    {
        public LedgerDbContext(DbContextOptions<LedgerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name)
                      .IsRequired();
                // Description is optional
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username)
                      .IsRequired();
                entity.Property(u => u.Email)
                      .IsRequired();
                entity.Property(u => u.PasswordHash)
                      .IsRequired();
                entity.Property(u => u.DisplayName)
                      .IsRequired();
                entity.HasIndex(u => u.Username)
                      .IsUnique();
                entity.HasIndex(u => u.Email)
                      .IsUnique();
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Date)
                      .IsRequired();
                entity.Property(t => t.Type)
                      .IsRequired();
                entity.Property(t => t.DkkAmount)
                      .HasColumnType("numeric(18,2)")
                      .IsRequired();

                // Configure the owned Money type for Amount.
                // This will map Money’s properties as columns on the Transactions table.
                entity.OwnsOne(t => t.Amount, money =>
                {
                    money.Property(m => m.Galleons)
                         .HasColumnName("Galleons")
                         .HasColumnType("integer");
                    money.Property(m => m.Sickles)
                         .HasColumnName("Sickles")
                         .HasColumnType("integer");
                    money.Property(m => m.Knuts)
                         .HasColumnName("Knuts")
                         .HasColumnType("integer");
                });

                // Configure relationships for foreign keys:
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ExchangeRate
            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                  entity.HasKey(er => er.Id);

                  entity.Property(er => er.GalleonToDkk)
                        .IsRequired()
                        .HasColumnType("numeric(18,2)");

                  entity.Property(er => er.SickleToDkk)
                        .IsRequired()
                        .HasColumnType("numeric(18,2)");

                  entity.Property(er => er.KnutToDkk)
                        .IsRequired()
                        .HasColumnType("numeric(18,2)");

                  entity.Property(er => er.EffectiveDate)
                        .IsRequired();
            });

            // Configure RecurringTransaction
            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Description)
                      .IsRequired();
                entity.Property(rt => rt.DkkAmount)
                      .IsRequired()
                      .HasColumnType("numeric(18,2)");
                entity.Property(rt => rt.Galleons)
                      .IsRequired();
                entity.Property(rt => rt.Sickles)
                      .IsRequired();
                entity.Property(rt => rt.Knuts)
                      .IsRequired();
                entity.Property(rt => rt.Frequency)
                      .IsRequired();
                entity.Property(rt => rt.NextOccurrence)
                      .IsRequired();
            });
        }
    }
}

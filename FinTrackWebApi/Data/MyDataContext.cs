using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;
using DocumentFormat.OpenXml.VariantTypes;

namespace FinTrackWebApi.Data
{
    public class MyDataContext : DbContext
    {
        public MyDataContext(DbContextOptions<MyDataContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; } = null!;
        public DbSet<OtpVerificationModel> OtpVerification { get; set; } = null!;
        public DbSet<UserSettingsModel> UserSettings { get; set; } = null!;
        public DbSet<CategoryModel> Categories { get; set; } = null!;
        public DbSet<BudgetCategoryModel> BudgetCategories { get; set; } = null!;
        public DbSet<BudgetModel> Budgets { get; set; } = null!;
        public DbSet<TransactionModel> Transactions { get; set; } = null!;
        public DbSet<AccountModel> Accounts { get; set; } = null!;

        public DbSet<CurrencySnapshotModel> CurrencySnapshots { get; set; } = null!;
        public DbSet<ExchangeRateModel> ExchangeRates { get; set; } = null!;
        public DbSet<CurrencyModel> Currencies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserId);

                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Settings)
                      .WithOne(s => s.User)
                      .HasForeignKey<UserSettingsModel>(s => s.UserId);

                entity.HasMany(u => u.Budgets)
                      .WithOne(b => b.User)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Categories)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OtpVerificationModel>(entity =>
            {
                entity.ToTable("OtpVerification");
                entity.HasKey(o => o.OtpId);
                entity.HasIndex(o => o.Email);
            });

            modelBuilder.Entity<UserSettingsModel>(entity =>
            {
                entity.ToTable("UserSettings");
                entity.HasKey(s => s.SettingsId);

                entity.HasIndex(s => s.UserId).IsUnique();

                entity.HasOne(s => s.User)
                      .WithOne(u => u.Settings)
                      .HasForeignKey<UserSettingsModel>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CategoryModel>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.Name).HasColumnName("CategoryName");

                entity.Property(c => c.Type)
                      .HasColumnName("CategoryType")
                      .HasConversion<string>();

                entity.HasIndex(c => new { c.UserId, c.Name, c.Type }).IsUnique();

                entity.HasMany(c => c.BudgetAllocations)
                      .WithOne(bc => bc.Category)
                      .HasForeignKey(bc => bc.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BudgetModel>(entity =>
            {
                entity.ToTable("Budgets");
                entity.HasKey(b => b.BudgetId);

                entity.Property(b => b.Name).HasColumnName("BudgetName");

                entity.Property(b => b.Description).IsRequired(false);

                entity.HasMany(b => b.BudgetCategories)
                      .WithOne(bc => bc.Budget)
                      .HasForeignKey(bc => bc.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BudgetCategoryModel>(entity =>
            {
                entity.ToTable("BudgetCategories");
                entity.HasKey(bc => bc.BudgetCategoryId);

                entity.Property(bc => bc.AllocatedAmount)
                      .HasColumnType("decimal(18, 2)");

                entity.HasIndex(bc => new { bc.BudgetId, bc.CategoryId }).IsUnique();
            });

            modelBuilder.Entity<TransactionModel>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(t => t.TransactionId);

                entity.Property(t => t.Amount)
                      .HasColumnType("decimal(18, 2)");

                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CategoryId);
                entity.HasIndex(t => t.TransactionDateUtc);
                entity.HasIndex(t => t.AccountId);
            });

            modelBuilder.Entity<AccountModel>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(a => a.AccountId);
                entity.Property(a => a.Balance)
                      .HasColumnType("decimal(18, 2)");
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Accounts)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(a => a.UserId);
            });

            modelBuilder.Entity<ExchangeRateModel>(entity =>
            {
                entity.ToTable("ExchangeRates");
                entity.HasKey(e => e.ExchangeRateId);

                entity.Property(e => e.Rate).HasColumnType("numeric(18, 6)");

                entity.HasOne(d => d.Currency)
                      .WithMany(p => p.ExchangeRates)
                      .HasForeignKey(d => d.CurrencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CurrencySnapshotId, e.CurrencyId })
                      .IsUnique();
            });

            modelBuilder.Entity<CurrencySnapshotModel>(entity => {
                entity.ToTable("CurrencySnapshots");
                entity.HasKey(cs => cs.CurrencySnapshotId);

                entity.Property(cs => cs.BaseCurrency)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.HasMany(cs => cs.Rates)
                      .WithOne(er => er.CurrencySnapshot)
                      .HasForeignKey(er => er.CurrencySnapshotId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cs => cs.FetchTimestamp);
            });

            modelBuilder.Entity<CurrencyModel>(entity =>
            {
                entity.HasKey(c => c.CurrencyId);

                entity.Property(c => c.Code)
                      .IsRequired()
                      .HasMaxLength(10);
                entity.HasIndex(c => c.Code).IsUnique();

                entity.Property(c => c.Name).HasMaxLength(100);
                entity.Property(c => c.CountryCode).HasMaxLength(10);
                entity.Property(c => c.CountryName).HasMaxLength(100);
                entity.Property(c => c.Status).HasMaxLength(20);
                entity.Property(c => c.IconUrl).HasMaxLength(255);

                // entity.Property(c => c.AvailableFrom).HasColumnType("date");
                // entity.Property(c => c.AvailableUntil).HasColumnType("date");

                entity.Property(c => c.LastUpdatedUtc).IsRequired();
            });
        }
    }
}
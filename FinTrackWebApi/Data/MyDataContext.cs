using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;

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
        }
    }
}
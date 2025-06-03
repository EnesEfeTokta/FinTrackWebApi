using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Data
{
    public class MyDataContext : IdentityDbContext<UserModel, IdentityRole<int>, int>
    {
        public MyDataContext(DbContextOptions<MyDataContext> options) : base(options) { }

        public DbSet<OtpVerificationModel> OtpVerification { get; set; }
        public DbSet<UserSettingsModel> UserSettings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<BudgetCategoryModel> BudgetCategories { get; set; }
        public DbSet<BudgetModel> Budgets { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<CurrencySnapshotModel> CurrencySnapshots { get; set; }
        public DbSet<ExchangeRateModel> ExchangeRates { get; set; }
        public DbSet<CurrencyModel> Currencies { get; set; }
        public DbSet<MembershipPlanModel> MembershipPlans { get; set; }
        public DbSet<UserMembershipModel> UserMemberships { get; set; }
        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<DebtModel> Debts { get; set; }
        public DbSet<VideoMetadataModel> VideoMetadatas { get; set; }

        public DbSet<EmployeesModel> Employees { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<EmployeeDepartmentModel> EmployeeDepartments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.ToTable("Users");

                entity.HasOne(u => u.Settings)
                      .WithOne(s => s.User)
                      .HasForeignKey<UserSettingsModel>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Budgets)
                      .WithOne(b => b.User)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Categories)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Transactions)
                      .WithOne(t => t.User)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Accounts)
                      .WithOne(a => a.User)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.UserMemberships)
                      .WithOne(um => um.User)
                      .HasForeignKey(um => um.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Payments)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(u => u.Notifications)
                        .WithOne(n => n.User)
                        .HasForeignKey(n => n.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.DebtsAsLender)
                        .WithOne(d => d.Lender)
                        .HasForeignKey(d => d.LenderId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.DebtsAsBorrower)
                        .WithOne(d => d.Borrower)
                        .HasForeignKey(d => d.BorrowerId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.UploadedVideos)
                        .WithOne(v => v.UploadedUser)
                        .HasForeignKey(v => v.UploadedByUserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OtpVerificationModel>(entity =>
            {
                entity.ToTable("OtpVerification");
                entity.HasKey(o => o.OtpId);
                entity.Property(o => o.OtpId).ValueGeneratedOnAdd();
                entity.HasIndex(o => o.Email);
            });

            modelBuilder.Entity<UserSettingsModel>(entity =>
            {
                entity.ToTable("UserSettings");
                entity.HasKey(s => s.SettingsId);
                entity.Property(s => s.SettingsId).ValueGeneratedOnAdd();
                entity.HasIndex(s => s.UserId).IsUnique();
            });

            modelBuilder.Entity<CategoryModel>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.CategoryId);
                entity.Property(c => c.CategoryId).ValueGeneratedOnAdd();
                entity.Property(c => c.Name).HasColumnName("CategoryName").IsRequired();
                entity.Property(c => c.Type).HasColumnName("CategoryType").HasConversion<string>().IsRequired();
                entity.HasIndex(c => new { c.UserId, c.Name, c.Type }).IsUnique();

                entity.HasMany(c => c.BudgetAllocations)
                      .WithOne(bc => bc.Category)
                      .HasForeignKey(bc => bc.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Transactions)
                      .WithOne(t => t.Category)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BudgetModel>(entity =>
            {
                entity.ToTable("Budgets");
                entity.HasKey(b => b.BudgetId);
                entity.Property(b => b.BudgetId).ValueGeneratedOnAdd();
                entity.Property(b => b.Name).HasColumnName("BudgetName").IsRequired();
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
                entity.Property(bc => bc.BudgetCategoryId).ValueGeneratedOnAdd();
                entity.Property(bc => bc.AllocatedAmount).HasColumnType("decimal(18, 2)").IsRequired();
                entity.HasIndex(bc => new { bc.BudgetId, bc.CategoryId }).IsUnique();
            });

            modelBuilder.Entity<TransactionModel>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.TransactionId).ValueGeneratedOnAdd();
                entity.Property(t => t.Amount).HasColumnType("decimal(18, 2)").IsRequired();
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CategoryId);
                entity.HasIndex(t => t.TransactionDateUtc);
                entity.HasIndex(t => t.AccountId);
            });

            modelBuilder.Entity<AccountModel>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(a => a.AccountId);
                entity.Property(a => a.AccountId).ValueGeneratedOnAdd();
                entity.Property(a => a.Balance).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(a => a.Name).IsRequired();
                entity.HasIndex(a => new { a.UserId, a.Name }).IsUnique();
            });

            modelBuilder.Entity<ExchangeRateModel>(entity =>
            {
                entity.ToTable("ExchangeRates");
                entity.HasKey(e => e.ExchangeRateId);
                entity.Property(e => e.ExchangeRateId).ValueGeneratedOnAdd();
                entity.Property(e => e.Rate).HasColumnType("decimal(18, 6)").IsRequired();
                entity.HasIndex(e => new { e.CurrencySnapshotId, e.CurrencyId }).IsUnique();
            });

            modelBuilder.Entity<CurrencySnapshotModel>(entity => {
                entity.ToTable("CurrencySnapshots");
                entity.HasKey(cs => cs.CurrencySnapshotId);
                entity.Property(cs => cs.CurrencySnapshotId).ValueGeneratedOnAdd();
                entity.Property(cs => cs.BaseCurrency).IsRequired().HasMaxLength(20);
                entity.HasMany(cs => cs.Rates)
                      .WithOne(er => er.CurrencySnapshot)
                      .HasForeignKey(er => er.CurrencySnapshotId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(cs => cs.FetchTimestamp);
            });

            modelBuilder.Entity<CurrencyModel>(entity =>
            {
                entity.ToTable("Currencies");
                entity.HasKey(c => c.CurrencyId);
                entity.Property(c => c.CurrencyId)
                      .ValueGeneratedOnAdd();
                entity.Property(c => c.Code)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.HasIndex(c => c.Code)
                      .IsUnique();
                entity.Property(c => c.Name)
                      .HasMaxLength(100)
                      .IsRequired();
                entity.Property(c => c.CountryCode)
                      .HasMaxLength(20);
                entity.Property(c => c.CountryName)
                      .HasMaxLength(100);
                entity.Property(c => c.Status)
                      .HasMaxLength(20);
                entity.Property(c => c.IconUrl)
                      .HasMaxLength(255);
                entity.Property(c => c.LastUpdatedUtc)
                      .IsRequired();
            });

            modelBuilder.Entity<MembershipPlanModel>(entity =>
            {
                entity.ToTable("MembershipPlans");
                entity.HasKey(e => e.MembershipPlanId);
                entity.Property(e => e.MembershipPlanId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Description).IsRequired(false);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.BillingCycle).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DurationInDays).IsRequired(false);
                entity.Property(e => e.Features).IsRequired(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();

                entity.HasMany(p => p.UserMemberships)
                      .WithOne(um => um.Plan)
                      .HasForeignKey(um => um.MembershipPlanId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserMembershipModel>(entity =>
            {
                entity.ToTable("UserMemberships");
                entity.HasKey(e => e.UserMembershipId);
                entity.Property(e => e.UserMembershipId).ValueGeneratedOnAdd();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.MembershipPlanId);
                entity.HasIndex(e => e.EndDate);
            });

            modelBuilder.Entity<PaymentModel>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentId).ValueGeneratedOnAdd();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.PaymentDate).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.TransactionId).HasMaxLength(255).IsRequired(false);
                entity.HasIndex(e => e.TransactionId)
                      .IsUnique()
                      .HasFilter("\"TransactionId\" IS NOT NULL");

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UserMembershipId);

                entity.HasOne(p => p.UserMembership)
                      .WithMany(um => um.Payments)
                      .HasForeignKey(p => p.UserMembershipId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<NotificationModel>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.NotificationId).ValueGeneratedOnAdd();
                entity.Property(n => n.MessageHead).IsRequired().HasMaxLength(200);
                entity.Property(n => n.MessageBody).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.CreatedAtUtc).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
                entity.HasIndex(n => n.UserId);
            });

            modelBuilder.Entity<DebtModel>(entity =>
            {
                entity.ToTable("Debts");
                entity.HasKey(d => d.DebtId);
                entity.Property(d => d.DebtId).ValueGeneratedOnAdd();

                entity.Property(d => d.Amount).HasColumnType("decimal(18, 2)").IsRequired();

                entity.HasOne(d => d.CurrencyModel)
                      .WithMany()
                      .HasForeignKey(d => d.CurrencyId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.Description).HasMaxLength(500).IsRequired();
                entity.Property(d => d.CreateAtUtc).IsRequired();
                entity.Property(d => d.DueDateUtc).IsRequired();
                entity.Property(d => d.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasOne(d => d.Lender)
                      .WithMany(u => u.DebtsAsLender)
                      .HasForeignKey(d => d.LenderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Borrower)
                      .WithMany(u => u.DebtsAsBorrower)
                      .HasForeignKey(d => d.BorrowerId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.VideoMetadata)
                      .WithOne(vm => vm.Debt)
                      .HasForeignKey<DebtModel>(d => d.VideoMetadataId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<VideoMetadataModel>(entity =>
            {
                entity.ToTable("VideoMetadatas");
                entity.HasKey(v => v.VideoMetadataId);
                entity.Property(v => v.VideoMetadataId).ValueGeneratedOnAdd();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.StorageType).HasConversion<string>();
                entity.HasOne(entity => entity.UploadedUser)
                      .WithMany(user => user.UploadedVideos)
                      .HasForeignKey(entity => entity.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmployeesModel>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EmployeeStatus).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HireDate).IsRequired();
                entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)").IsRequired();
            });

            modelBuilder.Entity<DepartmentModel>(entity =>
            {
                entity.ToTable("Departments");
                entity.HasKey(d => d.DepartmentId);
                entity.Property(d => d.DepartmentId).ValueGeneratedOnAdd();
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<EmployeeDepartmentModel>(entity =>
            {
                entity.ToTable("EmployeeDepartments");
                entity.HasKey(ed => new { ed.EmployeeId, ed.DepartmentId });
                entity.HasOne(ed => ed.Employee)
                      .WithMany(e => e.EmployeeDepartments)
                      .HasForeignKey(ed => ed.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ed => ed.Department)
                      .WithMany(d => d.EmployeeDepartments)
                      .HasForeignKey(ed => ed.DepartmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
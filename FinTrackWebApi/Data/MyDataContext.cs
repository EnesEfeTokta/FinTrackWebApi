using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using FinTrackWebApi.Models.Account;
using FinTrackWebApi.Models.Budget;
using FinTrackWebApi.Models.Category;
using FinTrackWebApi.Models.Currency;
using FinTrackWebApi.Models.Debt;
using FinTrackWebApi.Models.Empoyee;
using FinTrackWebApi.Models.Feedback;
using FinTrackWebApi.Models.Membership;
using FinTrackWebApi.Models.Otp;
using FinTrackWebApi.Models.Tranaction;
using FinTrackWebApi.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Data
{
    public class MyDataContext : IdentityDbContext<UserModel, IdentityRole<int>, int>
    {
        public MyDataContext(DbContextOptions<MyDataContext> options)
            : base(options) { }

        public DbSet<OtpVerificationModel> OtpVerification { get; set; }
        public DbSet<UserAppSettingsModel> UserAppSettings { get; set; }
        public DbSet<UserNotificationSettingsModel> UserNotificationSettings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<BudgetModel> Budgets { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<TransactionCategoryModel> TransactionCategories { get; set; }
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
        public DbSet<DebtVideoMetadataModel> DebtVideoMetadatas { get; set; }
        public DbSet<FeedbackModel> Feedbacks { get; set; }

        public DbSet<EmployeesModel> Employees { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<EmployeeDepartmentModel> EmployeeDepartments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Users");
                entity.Property(u => u.ProfilePicture)
                        .HasColumnName("ProfilePicture")
                        .HasMaxLength(255)
                        .IsRequired(false)
                        .HasDefaultValue("https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740");
                entity.Property(u => u.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);

                // -- İlişkiler --
                entity
                    .HasOne(u => u.AppSettings)
                    .WithOne(s => s.User)
                    .HasForeignKey<UserAppSettingsModel>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasOne(u => u.NotificationSettings)
                    .WithOne(s => s.User)
                    .HasForeignKey<UserNotificationSettingsModel>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.Budgets)
                    .WithOne(b => b.User)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.Categories)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.Transactions)
                    .WithOne(t => t.User)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.Accounts)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.UserMemberships)
                    .WithOne(um => um.User)
                    .HasForeignKey(um => um.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.Payments)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity
                    .HasMany(u => u.Notifications)
                    .WithOne(n => n.User)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.DebtsAsLender)
                    .WithOne(d => d.Lender)
                    .HasForeignKey(d => d.LenderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.DebtsAsBorrower)
                    .WithOne(d => d.Borrower)
                    .HasForeignKey(d => d.BorrowerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(u => u.UploadedVideos)
                    .WithOne(v => v.UploadedUser)
                    .HasForeignKey(v => v.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(f => f.Feedbacks)
                    .WithOne(u => u.User)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OtpVerificationModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("OtpVerifications");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).ValueGeneratedOnAdd();
                entity.Property(o => o.Email)
                      .HasColumnName("Email")
                      .HasMaxLength(255)
                      .IsRequired(true);
                entity.Property(o => o.OtpCode)
                        .HasColumnName("OtpCode")
                        .HasMaxLength(255)
                        .IsRequired(true);
                entity.Property(o => o.CreateAt)
                        .HasColumnName("CreateAt")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(o => o.ExpireAt)
                        .HasColumnName("ExpireAt")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW() + INTERVAL '5 minutes'")
                        .IsRequired(true);
                entity.Property(o => o.Username)
                        .HasColumnName("Username")
                        .HasMaxLength(100)
                        .IsRequired(true);
                entity.Property(o => o.ProfilePicture)
                        .HasColumnName("ProfilePicture")
                        .HasMaxLength(255)
                        .IsRequired(false)
                        .HasDefaultValue("https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740");
                entity.Property(o => o.TemporaryPlainPassword)
                        .HasColumnName("TemporaryPlainPassword")
                        .HasMaxLength(255)
                        .IsRequired(true);

                // --Index --
                entity.HasIndex(o => o.Email);
            });

            modelBuilder.Entity<UserAppSettingsModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("UserAppSettings");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedOnAdd();
                entity.Property(s => s.Appearance)
                      .HasColumnName("Appearance")
                      .HasConversion<string>()
                      .HasDefaultValue(AppearanceType.Dark)
                      .IsRequired(true);
                entity.Property(s => s.BaseCurrency)
                      .HasColumnName("BaseCurrency")
                      .HasConversion<string>()
                      .HasDefaultValue(BaseCurrencyType.TRY)
                      .IsRequired(true);
                entity.Property(s => s.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(s => s.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(s => s.User)
                      .WithOne(u => u.AppSettings)
                      .HasForeignKey<UserAppSettingsModel>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // -- Index --
                entity.HasIndex(s => s.UserId).IsUnique();
            });

            modelBuilder.Entity<UserNotificationSettingsModel>(entiy =>
            {
                // -- Tablo --
                entiy.ToTable("UserNotificationSettings");
                entiy.HasKey(s => s.Id);
                entiy.Property(s => s.Id).ValueGeneratedOnAdd();
                entiy.Property(s => s.SpendingLimitWarning)
                      .HasColumnName("SpendingLimitWarning")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entiy.Property(s => s.ExpectedBillReminder)
                      .HasColumnName("ExpectedBillReminder")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entiy.Property(s => s.WeeklySpendingSummary)
                      .HasColumnName("WeeklySpendingSummary")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entiy.Property(s => s.NewFeaturesAndAnnouncements)
                      .HasColumnName("NewFeaturesAndAnnouncements")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entiy.Property(s => s.EnableDesktopNotifications)
                      .HasColumnName("EnableDesktopNotifications")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entiy.Property(s => s.CreatedAtUtc)
                      .HasColumnName("CreatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .HasDefaultValueSql("NOW()")
                      .IsRequired(true);
                entiy.Property(s => s.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entiy.HasOne(s => s.User)
                      .WithOne(u => u.NotificationSettings)
                      .HasForeignKey<UserNotificationSettingsModel>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // -- Index --
                entiy.HasIndex(s => s.UserId).IsUnique();
            });

            modelBuilder.Entity<CategoryModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Name)
                      .HasColumnName("CategoryName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(c => c.CreatedAtUtc)
                      .HasColumnName("CreatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .HasDefaultValueSql("NOW()")
                      .IsRequired(true);
                entity.Property(c => c.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasMany(category => category.Budgets)
                      .WithOne(budget => budget.Category)
                      .HasForeignKey(budget => budget.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(c => new { c.UserId, c.Name })
                      .IsUnique();
            });

            modelBuilder.Entity<BudgetModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Budgets");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Id).ValueGeneratedOnAdd();
                entity.Property(b => b.Name)
                      .HasColumnName("BudgetName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(b => b.Description)
                      .HasColumnName("Description")
                      .HasMaxLength(500)
                      .IsRequired(false);
                entity.Property(b => b.AllocatedAmount)
                        .HasColumnName("AllocatedAmount")
                        .HasColumnType("decimal(18, 2)")
                        .IsRequired(true);
                entity.Property(b => b.Currency)
                      .HasColumnName("Currency")
                      .HasConversion<string>()
                      .HasDefaultValue(BaseCurrencyType.TRY)
                      .IsRequired(true);
                entity.Property(b => b.StartDate)
                      .HasColumnName("StartDate")
                      .HasColumnType("date")
                      .IsRequired(true);
                entity.Property(b => b.EndDate)
                      .HasColumnName("EndDate")
                      .HasColumnType("date")
                      .IsRequired(true);
                entity.Property(b => b.IsActive)
                        .HasColumnName("IsActive")
                        .HasDefaultValue(true)
                        .IsRequired(true);
                entity.Property(b => b.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(b => b.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Budgets)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(budget => budget.Category)
                        .WithMany(category => category.Budgets)
                        .HasForeignKey(budget => budget.CategoryId)
                        .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TransactionModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Transactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.Amount)
                      .HasColumnName("Amount")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);
                entity.Property(t => t.Currency)
                        .HasColumnName("Currency")
                        .HasConversion<string>()
                        .HasDefaultValue(BaseCurrencyType.TRY)
                        .IsRequired(true);
                entity.Property(t => t.TransactionDateUtc)
                      .HasColumnName("TransactionDateUtc")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired(true);
                entity.Property(t => t.Description)
                        .HasColumnName("Description")
                        .HasMaxLength(500)
                        .IsRequired(false);
                entity.Property(t => t.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(t => t.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Account)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                // --Index --
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CategoryId);
                entity.HasIndex(t => t.TransactionDateUtc);
                entity.HasIndex(t => t.AccountId);
            });

            modelBuilder.Entity<TransactionCategoryModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("TransactionCategories");
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.Id).ValueGeneratedOnAdd();
                entity.Property(tc => tc.Name)
                      .HasColumnName("CategoryName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(tc => tc.Type)
                      .HasColumnName("CategoryType")
                      .HasConversion<string>()
                      .IsRequired(true);
                entity.Property(tc => tc.CreatedAt)
                        .HasColumnName("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(tc => tc.UpdatedAt)
                        .HasColumnName("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(tc => tc.User)
                      .WithMany(u => u.TransactionCategories)
                      .HasForeignKey(tc => tc.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(tc => tc.Transactions)
                      .WithOne(t => t.Category)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(tc => new { tc.UserId, tc.Name }).IsUnique();
            });

            modelBuilder.Entity<AccountModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Accounts");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedOnAdd();
                entity.Property(a => a.Name)
                      .HasColumnName("AccountName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(a => a.Type)
                      .HasColumnName("AccountType")
                      .HasConversion<string>()
                      .HasDefaultValue(AccountType.Cash)
                      .IsRequired(true);
                entity.Property(a => a.IsActive)
                      .HasColumnName("IsActive")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entity.Property(a => a.Balance)
                      .HasColumnName("Balance")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);
                entity.Property(a => a.Currency)
                      .HasColumnName("Currency")
                      .HasConversion<string>()
                      .HasDefaultValue(BaseCurrencyType.TRY)
                      .IsRequired(true);
                entity.Property(a => a.CreatedAtUtc)
                      .HasColumnName("CreatedAtUtc")
                      .HasDefaultValueSql("NOW()")
                      .IsRequired(true);
                entity.Property(a => a.UpdatedAtUtc)
                      .HasColumnName("UpdatedAtUtc")
                      .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Accounts)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(a => a.Transactions)
                      .WithOne(t => t.Account)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);

                // -- Index --
                entity.HasIndex(a => new { a.UserId, a.Name }).IsUnique();
            });

            modelBuilder.Entity<ExchangeRateModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("ExchangeRates");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Rate)
                      .HasColumnType("decimal(18, 6)")
                      .IsRequired(true);

                // -- İlişkiler --
                entity.HasOne(e => e.CurrencySnapshot)
                      .WithMany(cs => cs.Rates)
                      .HasForeignKey(e => e.CurrencySnapshotId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Currency)
                        .WithMany(c => c.ExchangeRates)
                        .HasForeignKey(e => e.CurrencyId)
                        .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(e => new { e.CurrencySnapshotId, e.CurrencyId }).IsUnique();
            });

            modelBuilder.Entity<CurrencySnapshotModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("CurrencySnapshots");
                entity.HasKey(cs => cs.Id);
                entity.Property(cs => cs.Id).ValueGeneratedOnAdd();
                entity.Property(cs => cs.BaseCurrency)
                      .IsRequired(true)
                      .HasMaxLength(20);
                entity.Property(cs => cs.FetchTimestamp)
                        .HasColumnName("FetchTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(true)
                        .HasDefaultValueSql("NOW()");

                // -- İlişkiler --
                entity
                    .HasMany(cs => cs.Rates)
                    .WithOne(er => er.CurrencySnapshot)
                    .HasForeignKey(er => er.CurrencySnapshotId)
                    .OnDelete(DeleteBehavior.Cascade);

                // -- Index --
                entity.HasIndex(cs => cs.FetchTimestamp);
            });

            modelBuilder.Entity<CurrencyModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Currencies");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Code)
                      .HasColumnName("CurrencyCode")
                      .HasMaxLength(20)
                      .IsRequired(true);
                entity.Property(c => c.Name)
                      .HasColumnName("CurrencyName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(c => c.CountryCode)
                        .HasColumnName("CountryCode")
                        .HasMaxLength(20)
                        .IsRequired(false);
                entity.Property(c => c.CountryName)
                        .HasColumnName("CountryName")
                        .HasMaxLength(100)
                        .IsRequired(false);
                entity.Property(c => c.Status)
                        .HasColumnName("Status")
                        .HasMaxLength(20)
                        .IsRequired(true);
                entity.Property(c => c.LastUpdatedUtc)
                        .HasColumnName("LastUpdatedUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(true)
                        .HasDefaultValueSql("NOW()");
                entity.Property(c => c.AvailableFrom)
                        .HasColumnName("AvailableFrom")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(true)
                        .HasDefaultValueSql("NOW() - INTERVAL '1 year'");
                entity.Property(c => c.AvailableUntil)
                        .HasColumnName("AvailableUntil")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false)
                        .HasDefaultValueSql("NOW() + INTERVAL '1 year'");
                entity.Property(c => c.IconUrl)
                        .HasColumnName("IconUrl")
                        .HasMaxLength(255)
                        .IsRequired(false)
                        .HasDefaultValue("https://currencyfreaks.com/photos/flags/usd.png");

                // -- İlişkiler --
                entity.HasMany(c => c.ExchangeRates)
                    .WithOne(er => er.Currency)
                    .HasForeignKey(er => er.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(c => c.Code).IsUnique();
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.CountryCode).HasMaxLength(20);
                entity.Property(c => c.CountryName).HasMaxLength(100);
                entity.Property(c => c.Status).HasMaxLength(20);
                entity.Property(c => c.IconUrl).HasMaxLength(255);
                entity.Property(c => c.LastUpdatedUtc).IsRequired();
            });

            modelBuilder.Entity<MembershipPlanModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("MembershipPlans");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                      .HasColumnName("PlanName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(e => e.Description)
                      .HasColumnName("Description")
                      .HasMaxLength(500)
                      .IsRequired(true);
                entity.Property(e => e.Price)
                      .HasColumnName("Price")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);
                entity.Property(e => e.Currency)
                      .HasColumnName("Currency")
                      .HasMaxLength(5)
                      .HasConversion<string>()
                      .HasDefaultValue(BaseCurrencyType.TRY)
                      .IsRequired(true);
                entity.Property(e => e.BillingCycle)
                      .HasColumnName("BillingCycle")
                      .HasConversion<string>()
                      .HasDefaultValue(BillingCycleType.Monthly)
                      .IsRequired(true)
                      .HasMaxLength(50);
                entity.Property(e => e.DurationInDays)
                      .HasColumnName("DurationInDays")
                      .IsRequired(false);
                entity.Property(e => e.IsActive)
                      .HasColumnName("IsActive")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entity.Property(e => e.CreatedAt)
                        .HasColumnName("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(e => e.UpdatedAt)
                        .HasColumnName("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);
                entity.Property(e => e.PrioritySupport)
                        .HasColumnName("PrioritySupport")
                        .HasDefaultValue(false)
                        .IsRequired(true);

                // -- Ek Özellikler --
                entity.OwnsOne(e => e.Reporting, reportingBuilder =>
                {
                    reportingBuilder.Property(r => r.Level)
                             .HasColumnName("ReportingLevel")
                             .HasMaxLength(50)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportPdf)
                             .HasColumnName("CanExportPdf")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportWord)
                             .HasColumnName("CanExportWord")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportMarkdown)
                             .HasColumnName("CanExportMarkdown")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportXml)
                             .HasColumnName("CanExportXml")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportText)
                             .HasColumnName("CanExportText")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    reportingBuilder.Property(r => r.CanExportXlsx)
                             .HasColumnName("CanExportXlsx")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                });
                entity.OwnsOne(e => e.Emailing, emailingBuilder =>
                {
                    emailingBuilder.Property(e => e.CanEmailReports)
                             .HasColumnName("CanEmailReports")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    emailingBuilder.Property(e => e.MaxEmailsPerMonth)
                             .HasColumnName("MaxEmailsPerMonth")
                             .HasDefaultValue(0)
                             .IsRequired(true);
                });
                entity.OwnsOne(e => e.Budgeting, budgetingBuilder =>
                {
                    budgetingBuilder.Property(b => b.CanCreateBudgets)
                             .HasColumnName("CanCreateBudgets")
                             .HasDefaultValue(false)
                             .IsRequired(true);
                    budgetingBuilder.Property(b => b.MaxBudgets)
                             .HasColumnName("MaxBudgets")
                             .HasDefaultValue(0)
                             .IsRequired(true);
                });
                entity.OwnsOne(e => e.Accounts, accountsBuilder =>
                {
                    accountsBuilder.Property(a => a.MaxBankAccounts)
                             .HasColumnName("MaxBankAccounts")
                             .HasDefaultValue(0)
                             .IsRequired(true);
                });

                // -- Navigation Özellikleri --
                entity.Navigation(plan => plan.Reporting).IsRequired(false);
                entity.Navigation(plan => plan.Emailing).IsRequired(false);
                entity.Navigation(plan => plan.Budgeting).IsRequired(false);
                entity.Navigation(plan => plan.Accounts).IsRequired(false);

                // -- İlişkiler --
                entity
                    .HasMany(p => p.UserMemberships)
                    .WithOne(um => um.Plan)
                    .HasForeignKey(um => um.MembershipPlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<UserMembershipModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("UserMemberships");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Status)
                      .HasColumnName("Status")
                      .HasConversion<string>()
                      .HasDefaultValue(MembershipStatusType.PendingPayment)
                      .HasMaxLength(50)
                      .IsRequired(true);
                entity.Property(e => e.StartDate)
                        .HasColumnName("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(true)
                        .HasDefaultValueSql("NOW()");
                entity.Property(e => e.EndDate)
                        .HasColumnName("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(true);
                entity.Property(e => e.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(e => e.AutoRenew)
                      .HasColumnName("AutoRenew")
                      .HasDefaultValue(false)
                      .IsRequired(true);
                entity.Property(e => e.CancellationDate)
                        .HasColumnName("CancellationDate")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserMemberships)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Plan)
                        .WithMany(p => p.UserMemberships)
                        .HasForeignKey(e => e.MembershipPlanId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Payments)
                        .WithOne(p => p.UserMembership)
                        .HasForeignKey(p => p.UserMembershipId)
                        .OnDelete(DeleteBehavior.SetNull);

                // -- Index --
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.MembershipPlanId);
                entity.HasIndex(e => e.EndDate);
            });

            modelBuilder.Entity<PaymentModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Amount)
                      .HasColumnName("Amount")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);
                entity.Property(e => e.Currency)
                      .HasColumnName("Currency")
                      .HasConversion<string>()
                      .IsRequired(true)
                      .HasMaxLength(5)
                      .HasDefaultValue(BaseCurrencyType.TRY);
                entity.Property(e => e.PaymentDate)
                      .HasColumnName("PaymentDate")
                      .IsRequired(true);
                entity.Property(e => e.PaymentMethod)
                      .HasColumnName("PaymentMethod")
                      .HasMaxLength(100)
                      .IsRequired(false);
                entity.Property(e => e.TransactionId)
                      .HasColumnName("TransactionId")
                      .HasMaxLength(20)
                      .IsRequired(false);
                entity.Property(e => e.Status)
                      .HasColumnName("Status")
                      .HasConversion<string>()
                      .HasDefaultValue(PaymentStatusType.Pending)
                      .HasMaxLength(50)
                      .IsRequired(true);
                entity.Property(e => e.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(e => e.GatewayResponse)
                        .HasColumnName("GatewayResponse")
                        .HasMaxLength(1000)
                        .IsRequired(false);
                entity.Property(e => e.Notes)
                        .HasColumnName("Notes")
                        .HasMaxLength(500)
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.UserMembership)
                    .WithMany(um => um.Payments)
                    .HasForeignKey(p => p.UserMembershipId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                // -- Index --
                entity.HasIndex(e => e.TransactionId).IsUnique().HasFilter("\"TransactionId\" IS NOT NULL");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UserMembershipId);
            });

            modelBuilder.Entity<NotificationModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).ValueGeneratedOnAdd();
                entity.Property(n => n.MessageHead)
                      .HasColumnName("MessageHead")
                      .IsRequired(true)
                      .HasMaxLength(200);
                entity.Property(n => n.MessageBody)
                      .HasColumnName("MessageBody")
                      .IsRequired(true)
                      .HasMaxLength(1000);
                entity.Property(n => n.Type)
                        .HasColumnName("Type")
                        .HasConversion<string>()
                        .HasDefaultValue(NotificationType.Info)
                        .IsRequired(true)
                        .HasMaxLength(50);
                entity.Property(n => n.CreatedAtUtc)
                      .HasColumnName("CreatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired(true)
                      .HasDefaultValueSql("NOW()");
                entity.Property(n => n.IsRead)
                      .HasColumnName("IsRead")
                      .IsRequired(true)
                      .HasDefaultValue(false);
                entity.Property(n => n.ReadAtUtc)
                        .HasColumnName("ReadAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);
                entity.Property(n => n.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(n => n.UserId);
            });

            modelBuilder.Entity<DebtModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Debts");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).ValueGeneratedOnAdd();
                entity.Property(d => d.Amount)
                      .HasColumnName("Amount")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);
                entity.Property(d => d.Currency)
                        .HasColumnName("Currency")
                        .HasConversion<string>()
                        .HasMaxLength(5)
                        .HasDefaultValue(BaseCurrencyType.TRY)
                        .IsRequired(true);
                entity.Property(d => d.Description)
                      .HasColumnName("Description")
                      .HasMaxLength(500)
                      .IsRequired(true);
                entity.Property(d => d.CreateAtUtc)
                      .HasColumnName("CreateAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .HasDefaultValueSql("NOW()")
                      .IsRequired(true);
                entity.Property(d => d.UpdatedAtUtc)
                      .HasColumnName("UpdatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired(false);
                entity.Property(d => d.DueDateUtc)
                      .HasColumnName("DueDateUtc")
                      .HasColumnType("date")
                      .IsRequired(true);
                entity.Property(d => d.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .HasDefaultValue(DebtStatusType.PendingBorrowerAcceptance)
                      .IsRequired();
                entity.Property(d => d.PaidAtUtc)
                        .HasColumnName("PaidAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);
                entity.Property(d => d.OperatorApprovalAtUtc)
                        .HasColumnName("OperatorApprovalAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);
                entity.Property(d => d.BorrowerApprovalAtUtc)
                        .HasColumnName("BorrowerApprovalAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);
                entity.Property(d => d.PaymentConfirmationAtUtc)
                        .HasColumnName("PaymentConfirmationAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity
                    .HasOne(d => d.Lender)
                    .WithMany(u => u.DebtsAsLender)
                    .HasForeignKey(d => d.LenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(d => d.Borrower)
                    .WithMany(u => u.DebtsAsBorrower)
                    .HasForeignKey(d => d.BorrowerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VideoMetadataModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("VideoMetadatas");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).ValueGeneratedOnAdd();
                entity.Property(v => v.OriginalFileName)
                      .HasColumnName("OriginalFileName")
                      .HasMaxLength(255)
                      .IsRequired(false);
                entity.Property(v => v.StoredFileName)
                      .HasColumnName("StoredFileName")
                      .HasMaxLength(255)
                      .IsRequired(true);
                entity.Property(v => v.UnencryptedFilePath)
                        .HasColumnName("UnencryptedFilePath")
                        .HasMaxLength(500)
                        .IsRequired(false);
                entity.Property(v => v.EncryptedFilePath)
                        .HasColumnName("EncryptedFilePath")
                        .HasMaxLength(500)
                        .IsRequired(false);
                entity.Property(v => v.FileSize)
                        .HasColumnName("FileSize")
                        .HasColumnType("bigint")
                        .IsRequired(true);
                entity.Property(v => v.ContentType)
                        .HasColumnName("ContentType")
                        .HasMaxLength(100)
                        .IsRequired(true);
                entity.Property(v => v.UploadDateUtc)
                        .HasColumnName("UploadDateUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(v => v.Duration)
                        .HasColumnName("Duration")
                        .HasColumnType("interval")
                        .IsRequired(false);
                entity.Property(e => e.Status)
                      .HasColumnName("Status")
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .HasDefaultValue(VideoStatusType.PendingApproval)
                      .IsRequired(true);
                entity.Property(v => v.EncryptionKeyHash)
                        .HasColumnName("EncryptionKeyHash")
                        .HasMaxLength(100)
                        .IsRequired(false);
                entity.Property(v => v.EncryptionSalt)
                        .HasColumnName("EncryptionSalt")
                        .HasMaxLength(100)
                        .IsRequired(false);
                entity.Property(v => v.EncryptionIV)
                        .HasColumnName("EncryptionIV")
                        .HasMaxLength(100)
                        .IsRequired(false);
                entity.Property(v => v.StorageType)
                        .HasColumnName("StorageType")
                        .HasConversion<string>()
                        .HasDefaultValue(VideoStorageType.FileSystem)
                        .IsRequired(true)
                        .HasMaxLength(50);
                entity.Property(v => v.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(v => v.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(v => v.UploadedUser)
                      .WithMany(u => u.UploadedVideos)
                      .HasForeignKey(v => v.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(v => v.DebtVideoMetadatas)
                      .WithOne(dvm => dvm.VideoMetadata)
                      .HasForeignKey(dvm => dvm.VideoMetadataId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DebtVideoMetadataModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("DebtVideoMetadatas");
                entity.HasKey(dvm => dvm.Id);
                entity.Property(dvm => dvm.Id).ValueGeneratedOnAdd();
                entity.Property(dvm => dvm.Status)
                      .HasColumnName("Status")
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .HasDefaultValue(VideoStatusType.PendingApproval)
                      .IsRequired(true);
                entity.Property(dvm => dvm.CreatedAtUtc)
                        .HasColumnName("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(dvm => dvm.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired(false);

                // -- İlişkiler --
                entity
                    .HasOne(dvm => dvm.Debt)
                    .WithMany(d => d.DebtVideoMetadatas)
                    .HasForeignKey(dvm => dvm.DebtId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(dvm => dvm.VideoMetadata)
                    .WithMany(vm => vm.DebtVideoMetadatas)
                    .HasForeignKey(dvm => dvm.VideoMetadataId)
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
                entity
                    .HasOne(ed => ed.Employee)
                    .WithMany(e => e.EmployeeDepartments)
                    .HasForeignKey(ed => ed.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasOne(ed => ed.Department)
                    .WithMany(d => d.EmployeeDepartments)
                    .HasForeignKey(ed => ed.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FeedbackModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("Feedbacks");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).ValueGeneratedOnAdd();
                entity.Property(f => f.Subject)
                      .HasColumnName("Subject")
                      .HasMaxLength(255)
                      .IsRequired(true);
                entity.Property(f => f.Description)
                      .HasColumnName("Description")
                      .HasMaxLength(500)
                      .IsRequired(true);
                entity.Property(f => f.Type)
                       .HasColumnName("Type")
                       .HasConversion<string>()
                       .HasDefaultValue(FeedbackType.GeneralFeedback)
                       .IsRequired(true);
                entity.Property(f => f.SavedFilePath)
                      .HasColumnName("SavedFilePath")
                      .HasMaxLength(500)
                      .IsRequired(false);
                entity.Property(f => f.CreatedAtUtc)
                      .HasColumnName("CreatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .HasDefaultValueSql("NOW()")
                      .IsRequired(true);
                entity.Property(f => f.UpdatedAtUtc)
                      .HasColumnName("UpdatedAtUtc")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(fu => fu.User)
                      .WithMany(f => f.Feedbacks)
                      .HasForeignKey(fu => fu.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

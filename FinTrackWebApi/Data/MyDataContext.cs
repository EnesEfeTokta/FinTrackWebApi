using FinTrackWebApi.Models;
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
        public DbSet<DebtVideoMetadataModel> DebtVideoMetadatas { get; set; }

        public DbSet<EmployeesModel> Employees { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<EmployeeDepartmentModel> EmployeeDepartments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.ToTable("Users");

                entity
                    .HasOne(u => u.Settings)
                    .WithOne(s => s.User)
                    .HasForeignKey<UserSettingsModel>(s => s.UserId)
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
                // -- Tablo --
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Name)
                      .HasColumnName("CategoryName")
                      .HasMaxLength(100)
                      .IsRequired(true);
                entity.Property(c => c.Type)
                      .HasColumnName("CategoryType")
                      .HasConversion<string>()
                      .IsRequired(true);

                // -- İlişkiler --
                entity.HasMany(c => c.BudgetAllocations)
                      .WithOne(bc => bc.Category)
                      .HasForeignKey(bc => bc.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(c => c.Transactions)
                      .WithOne(t => t.Category)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -- Index --
                entity.HasIndex(c => new { c.UserId, c.Name, c.Type })
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
                        .HasDefaultValueSql("NOW()")
                        .IsRequired(true);
                entity.Property(b => b.UpdatedAtUtc)
                        .HasColumnName("UpdatedAtUtc")
                        .IsRequired(false);

                // -- İlişkiler --
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Budgets)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(b => b.BudgetCategories)
                      .WithOne(bc => bc.Budget)
                      .HasForeignKey(bc => bc.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BudgetCategoryModel>(entity =>
            {
                // -- Tablo --
                entity.ToTable("BudgetCategories");
                entity.HasKey(bc => bc.Id);
                entity.Property(bc => bc.Id).ValueGeneratedOnAdd();
                entity.Property(bc => bc.AllocatedAmount)
                      .HasColumnName("AllocatedAmount")
                      .HasColumnType("decimal(18, 2)")
                      .IsRequired(true);

                // -- İlişkiler --
                entity.HasOne(bc => bc.Budget)
                      .WithMany(b => b.BudgetCategories)
                      .HasForeignKey(bc => bc.BudgetId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(bc => bc.Category)
                      .WithMany(c => c.BudgetAllocations)
                      .HasForeignKey(bc => bc.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -- Inkdex --
                entity.HasIndex(bc => new { bc.BudgetId, bc.CategoryId })
                      .IsUnique();
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
                      .HasDefaultValue("Cash")
                      .IsRequired(true);
                entity.Property(a => a.IsActive)
                      .HasColumnName("IsActive")
                      .HasDefaultValue(true)
                      .IsRequired(true);
                entity.Property(a => a.Balance)
                      .HasColumnName("Balance")
                      .HasColumnType("decimal(18, 2)")
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
                entity.ToTable("ExchangeRates");
                entity.HasKey(e => e.ExchangeRateId);
                entity.Property(e => e.ExchangeRateId).ValueGeneratedOnAdd();
                entity.Property(e => e.Rate).HasColumnType("decimal(18, 6)").IsRequired();
                entity.HasIndex(e => new { e.CurrencySnapshotId, e.CurrencyId }).IsUnique();
            });

            modelBuilder.Entity<CurrencySnapshotModel>(entity =>
            {
                entity.ToTable("CurrencySnapshots");
                entity.HasKey(cs => cs.CurrencySnapshotId);
                entity.Property(cs => cs.CurrencySnapshotId).ValueGeneratedOnAdd();
                entity.Property(cs => cs.BaseCurrency).IsRequired().HasMaxLength(20);
                entity
                    .HasMany(cs => cs.Rates)
                    .WithOne(er => er.CurrencySnapshot)
                    .HasForeignKey(er => er.CurrencySnapshotId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(cs => cs.FetchTimestamp);
            });

            modelBuilder.Entity<CurrencyModel>(entity =>
            {
                entity.ToTable("Currencies");
                entity.HasKey(c => c.CurrencyId);
                entity.Property(c => c.CurrencyId).ValueGeneratedOnAdd();
                entity.Property(c => c.Code).IsRequired().HasMaxLength(20);
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
                entity.ToTable("MembershipPlans");
                entity.HasKey(e => e.MembershipPlanId);
                entity.Property(e => e.MembershipPlanId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Description).IsRequired(false);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)").IsRequired();
                entity
                    .Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("TRY");
                entity.Property(e => e.BillingCycle).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DurationInDays).IsRequired(false);
                entity.Property(e => e.Features).IsRequired(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();

                entity
                    .HasMany(p => p.UserMemberships)
                    .WithOne(um => um.Plan)
                    .HasForeignKey(um => um.MembershipPlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserMembershipModel>(entity =>
            {
                entity.ToTable("UserMemberships");
                entity.HasKey(e => e.UserMembershipId);
                entity.Property(e => e.UserMembershipId).ValueGeneratedOnAdd();

                entity
                    .Property(e => e.Status)
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
                entity
                    .Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("TRY");
                entity.Property(e => e.PaymentDate).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.TransactionId).HasMaxLength(255).IsRequired(false);
                entity
                    .HasIndex(e => e.TransactionId)
                    .IsUnique()
                    .HasFilter("\"TransactionId\" IS NOT NULL");

                entity
                    .Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UserMembershipId);

                entity
                    .HasOne(p => p.UserMembership)
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
                entity
                    .Property(n => n.CreatedAtUtc)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
                entity.HasIndex(n => n.UserId);
            });

            modelBuilder.Entity<DebtModel>(entity =>
            {
                entity.ToTable("Debts");
                entity.HasKey(d => d.DebtId);
                entity.Property(d => d.DebtId).ValueGeneratedOnAdd();

                entity.Property(d => d.Amount).HasColumnType("decimal(18, 2)").IsRequired();

                entity
                    .HasOne(d => d.CurrencyModel)
                    .WithMany()
                    .HasForeignKey(d => d.CurrencyId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.Description).HasMaxLength(500).IsRequired();
                entity.Property(d => d.CreateAtUtc).IsRequired();
                entity.Property(d => d.DueDateUtc).IsRequired();
                entity
                    .Property(d => d.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

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
                entity.ToTable("VideoMetadatas");
                entity.HasKey(v => v.VideoMetadataId);
                entity.Property(v => v.VideoMetadataId).ValueGeneratedOnAdd();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.StorageType).HasConversion<string>();

                entity
                    .HasOne(vm => vm.UploadedUser)
                    .WithMany(u => u.UploadedVideos)
                    .HasForeignKey(vm => vm.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DebtVideoMetadataModel>(entity =>
            {
                entity.ToTable("DebtVideoMetadatas");
                entity.HasKey(dvm => dvm.DebtVideoMetadataId);
                entity.Property(dvm => dvm.DebtVideoMetadataId).ValueGeneratedOnAdd();

                entity.Property(dvm => dvm.Status).HasConversion<string>();

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
        }
    }
}

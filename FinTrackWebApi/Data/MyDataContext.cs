using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Data
{
    public class MyDataContext: DbContext
    {
        public MyDataContext(DbContextOptions<MyDataContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<OtpVerificationModel> OtpVerifications { get; set; }
        public DbSet<UserSettingsModel> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserModel tablosu
            modelBuilder.Entity<UserModel>()
                .ToTable("Users")
                .HasKey(u => u.UserId);

            //OtpVerifications tablosu
            modelBuilder.Entity<OtpVerificationModel>()
                .ToTable("OtpVerifications")
                .HasKey(o => o.OtpId);

            // UserSettingsModel tablosu
            modelBuilder.Entity<UserSettingsModel>()
                .ToTable("UserSettings")
                .HasKey(s => s.SettingsId);

            // Usersettings ve UserModel arasındaki ilişkiyi tanımlıyoruz.
            modelBuilder.Entity<UserSettingsModel>()
                .HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettingsModel>(s => s.UserId);
        }
    }
}

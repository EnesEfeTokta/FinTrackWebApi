using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Data
{
    public class MyDataContext: DbContext
    {
        public MyDataContext(DbContextOptions<MyDataContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>()
                .ToTable("Users")
                .HasKey(u => u.UserId);
        }
    }
}

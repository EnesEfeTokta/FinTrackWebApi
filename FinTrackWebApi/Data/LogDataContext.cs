using FinTrackWebApi.Models.Logs;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Data
{
    public class LogDataContext : DbContext
    {
        public LogDataContext(DbContextOptions<LogDataContext> options) : base(options)
        {
        }

        public DbSet<AuditLogModel> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuditLogModel>().ToTable("AuditLogs");
        }
    }
}

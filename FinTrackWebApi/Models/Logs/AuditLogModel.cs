using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Models.Logs
{
    public class AuditLogModel
    {
        [Key]
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EntityId { get; set; } = string.Empty;

        public string Changes { get; set; } = string.Empty;
    }
}

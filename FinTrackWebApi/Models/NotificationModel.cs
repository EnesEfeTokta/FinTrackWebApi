using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public string MessageHead { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAtUtc { get; set; } = null;
        public DateTime? UpdatedAtUtc { get; set; }

    }
}

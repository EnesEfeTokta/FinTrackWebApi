using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class NotificationModel
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public string MessageHead { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

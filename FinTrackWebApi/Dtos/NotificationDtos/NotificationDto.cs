using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.NotificationDtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string MessageHead { get; set; } = null!;
        public string MessageBody { get; set; } = null!;
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

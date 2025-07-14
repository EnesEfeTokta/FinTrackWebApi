using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.NotificationDtos
{
    public class NotificationUpdateDto
    {
        public string MessageHead { get; set; } = null!;
        public string MessageBody { get; set; } = null!;
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
    }
}

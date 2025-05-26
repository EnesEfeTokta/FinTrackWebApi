namespace FinTrackWebApi.Dtos
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string MessageHead { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

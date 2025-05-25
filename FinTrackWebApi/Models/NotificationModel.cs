using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Models
{
    [Table("Notifications")]
    public class NotificationModel
    {
        [Key]
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual UserModel User { get; set; } = null!;

        [Required]
        [Column("MessageHead")]
        [MaxLength(100)]
        public string MessageHead { get; set; } = string.Empty;

        [Required]
        [Column("MessageBody")]
        [MaxLength(500)]
        public string MessageBody { get; set; } = string.Empty;

        [Required]
        [Column("NotificationType")]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [Column("CreatedAt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("IsRead")]
        public bool IsRead { get; set; } = false;
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}

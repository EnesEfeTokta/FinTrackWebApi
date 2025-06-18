using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Notifications")]
    public class NotificationModel
    {
        [Key]
        [Required]
        public string NotificationId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
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
        [Column("CreatedAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("IsRead")]
        public bool IsRead { get; set; } = false;
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success,
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("UserSettings")]
    public class UserSettingsModel
    {
        [Required]
        [Key]
        public int SettingsId { get; set; }

        [Required]
        [Column("Theme")]
        public string Theme { get; set; } = "light";

        [Required]
        [Column("Language")]
        public string Language { get; set; } = "tr";

        [Required]
        [Column("Currency")]
        public string Currency { get; set; } = "TRY";

        [Required]
        [Column("Notification")]
        public bool Notification { get; set; } = true;

        [Required]
        [Column("EntryDate")]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
    }
}

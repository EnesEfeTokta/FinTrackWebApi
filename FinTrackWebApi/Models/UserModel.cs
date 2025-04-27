using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Users")]
    public class UserModel
    {
        [Key]
        [Required]
        public int UserId { get; set; }

        [Required]
        [Column("Username")]
        public string Username { get; set; } = null!;

        [Required]
        [Column("Email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [Column("ProfilePicture")]
        public string ProfilePicture { get; set; } = "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740";

        [Required]
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public UserSettingsModel Settings { get; set; }
    }
}

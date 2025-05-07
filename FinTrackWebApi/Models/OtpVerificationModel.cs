using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("OtpVerifications")]
    public class OtpVerificationModel
    {
        [Required]
        [Key]
        public int OtpId { get; set; }

        [Required]
        [Column("Email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("OtpCode")]
        public string OtpCode { get; set; } = null!;

        [Required]
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("ExpireAt")]
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

        [Required]
        [Column("Username")]
        public string Username { get; set; } = null!;

        [Required]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [Column("ProfilePicture")]
        public string ProfilePicture { get; set; } = "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740";
    }
}

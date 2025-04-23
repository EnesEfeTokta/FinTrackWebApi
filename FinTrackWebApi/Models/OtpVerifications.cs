using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("OtpVerifications")]
    public class OtpVerifications
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
        [Column("IsVerified")]
        public bool IsVerified { get; set; } = false;

        [Required]
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("ExpireAt")]
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    }
}

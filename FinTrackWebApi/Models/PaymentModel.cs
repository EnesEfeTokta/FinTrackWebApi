using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Payments")]
    public class PaymentModel
    {
        [Required]
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual UserModel? User { get; set; }

        // [Required] // <<< BU ATTRIBUTE KALDIRILDI
        [ForeignKey("UserMembership")]
        public int? UserMembershipId { get; set; } // <<< NULLABLE (int?) YAPILDI
        public virtual UserMembershipModel? UserMembership { get; set; }

        [Required]
        [Column("PaymentDate")]
        [DataType(DataType.Date)] // Sadece tarih mi, yoksa saat de önemli mi? DateTime için DataType.DateTime daha uygun olabilir.
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("Currency")]
        public string Currency { get; set; } = string.Empty; // Varsayılan boş string yerine "TRY" gibi bir değer daha iyi olabilir

        [Column("PaymentMethod")] // Bu alan modelinizde yoktu, ekledim (genellikle olur)
        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [Column("TransactionId")] // Bu alan modelinizde yoktu, ekledim (genellikle olur)
        [MaxLength(255)]
        public string? TransactionId { get; set; }

        [Required]
        [Column("Status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Column("GatewayResponse")]
        public string? GatewayResponse { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2,
        Refunded = 3,
        PartiallyRefunded = 4,
    }
}

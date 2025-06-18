using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("DebtVideoMetadatas")]
    public class DebtVideoMetadataModel
    {
        [Key]
        public int DebtVideoMetadataId { get; set; }

        [Required]
        [ForeignKey("DebtId")]
        public int? DebtId { get; set; }
        public virtual DebtModel? Debt { get; set; }

        [Required]
        [ForeignKey("VideoMetadataId")]
        public int? VideoMetadataId { get; set; }
        public virtual VideoMetadataModel? VideoMetadata { get; set; }

        [Required]
        [Column("CreateAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UpdatedAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("Status")]
        [MaxLength(100)]
        public VideoStatus Status { get; set; } = VideoStatus.PendingApproval;
    }

    public enum VideoStatus
    {
        PendingApproval,                // Onay Bekliyor
        ApprovedAndQueuedForEncryption, // Operatör Onayladı, Şifreleme Kuyruğunda
        ProcessingEncryption,           // Şifreleniyor
        Encrypted,                      // Başarıyla Şifrelendi (ve anahtar gönderildi)
        Rejected,                       // Reddedildi
        EncryptionFailed,               // Şifreleme Başarısız Oldu
        ProcessingError                 // Genel Bir İşlem Hatası
    }
}

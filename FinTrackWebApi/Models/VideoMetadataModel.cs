using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("VideoMetadatas")]
    public class VideoMetadataModel
    {
        [Key]
        [Required]
        public int VideoMetadataId { get; set; }

        [Required]
        [ForeignKey(nameof(Debt))]
        public int DebtId { get; set; }
        public virtual DebtModel? Debt { get; set; }

        [Required]
        [ForeignKey(nameof(UploadedUser))]
        public int UploadedByUserId { get; set; }
        public virtual UserModel? UploadedUser { get; set; }

        [Column("OriginalFileName")]
        public string? OriginalFileName { get; set; }

        [Required]
        [Column("StoredFileName")]
        public string StoredFileName { get; set; } = string.Empty;

        [Column("UnencryptedFilePath")]
        public string? UnencryptedFilePath { get; set; }

        // Şifrelenmiş videonun tam dosya yolu
        [Column("EncryptedFilePath")]
        public string? EncryptedFilePath { get; set; }

        [Column("FileSize")]
        public long FileSize { get; set; } // Byte cinsinden dosya boyutu

        [Required]
        [Column("ContentType")]
        public string ContentType { get; set; } = string.Empty; // Örn: "video/mp4"

        [Required]
        [Column("UploadDateUtc")]
        public DateTime UploadDateUtc { get; set; } = DateTime.UtcNow;

        [Column("Duration")]
        public TimeSpan? Duration { get; set; } // Video süresi

        [Required]
        [Column("Status")]
        public VideoStatus Status { get; set; } = VideoStatus.PendingApproval;

        // Kullanıcının 20 karakterlik anahtarının HASH'i (örn: SHA256).
        [Column("EncryptionKeyHash")]
        public string? EncryptionKeyHash { get; set; }

        // PBKDF2 ile anahtar türetmek için kullanılan Salt değeri.
        [Column("EncryptionSalt")]
        public string? EncryptionSalt { get; set; }

        // AES şifrelemesi için kullanılan Initialization Vector.
        [Column("EncryptionIV")]
        public string? EncryptionIV { get; set; }

        [Required]
        [Column("StorageType")]
        public VideoStorageType StorageType { get; set; } = VideoStorageType.FileSystem; // Varsayılan değer
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

    public enum VideoStorageType
    {
        FileSystem,          // Dosya sisteminde şifresiz
        AzureBlob,           // Azure Blob'da şifresiz (kullanıyorsanız)
        EncryptedFileSystem  // Dosya sisteminde şifreli
    }
}
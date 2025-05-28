using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class VideoMetadataModel
    {
        [Key]
        [Required]
        public int VideoMetadataId { get; set; }

        [Required]
        [ForeignKey("DebtId")]
        public int DebtId { get; set; }
        public virtual DebtModel? Debt { get; set; }

        [Required]
        [ForeignKey("UploadedByUserId")]
        public int UploadedByUserId { get; set; }
        public virtual UserModel? UploadedUser { get; set; }

        [Column("OriginalFileName")]
        public string? OriginalFileName { get; set; }

        [Required]
        [Column("StoredFileName")]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [Column("FilePath")]
        public string FilePath { get; set; } = string.Empty;

        [Column("FileSize")]
        public long FileSize { get; set; }

        [Required]
        [Column("ContentType")]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        [Column("UploadDateUtc")]
        [DataType(DataType.DateTime)]
        public DateTime UploadDateUtc { get; set; } = DateTime.UtcNow;


        [Column("StorageType")]
        public VideoStorageType? StorageType { get; set; }

        [Column("Duration")]
        public TimeSpan? Duration { get; set; }
    }

    public enum VideoStorageType
    {
        FileSystem,
        AzureBlob,
        EncryptedFileSystem
    }
}
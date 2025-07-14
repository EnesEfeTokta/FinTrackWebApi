using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class VideoMetadataModel
    {
        public int Id { get; set; }
        public int UploadedByUserId { get; set; }
        public virtual UserModel? UploadedUser { get; set; }
        public string? OriginalFileName { get; set; }
        public string StoredFileName { get; set; } = string.Empty;
        public string? UnencryptedFilePath { get; set; }
        public string? EncryptedFilePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadDateUtc { get; set; } = DateTime.UtcNow;
        public TimeSpan? Duration { get; set; }
        public VideoStatusType Status { get; set; }
        public string? EncryptionKeyHash { get; set; }
        public string? EncryptionSalt { get; set; }
        public string? EncryptionIV { get; set; }
        public VideoStorageType StorageType { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public virtual ICollection<DebtVideoMetadataModel> DebtVideoMetadatas { get; set; } =
            new List<DebtVideoMetadataModel>();
    }
}

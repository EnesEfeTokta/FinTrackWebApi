using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class DebtVideoMetadataModel
    {
        public int Id { get; set; }
        public int? DebtId { get; set; }
        public virtual DebtModel? Debt { get; set; }
        public int? VideoMetadataId { get; set; }
        public virtual VideoMetadataModel? VideoMetadata { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public VideoStatusType Status { get; set; } = VideoStatusType.PendingApproval;
    }
}

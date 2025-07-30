using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.FeedbackDtos
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeedbackType? Type { get; set; }
        public string? SavedFilePath { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

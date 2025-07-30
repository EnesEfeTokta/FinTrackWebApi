using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.FeedbackDtos
{
    public class FeedbackUpdateDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeedbackType? Type { get; set; }
        public string? SavedFilePath { get; set; }
    }
}

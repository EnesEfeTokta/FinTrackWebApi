using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "The message field cannot be empty.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "The message must be at least 1 and at most 2000 characters.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "ClientChatSessionId field is required.")]
        public string? ClientChatSessionId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public string? UserId { get; set; }
        public string? AuthToken { get; set; }
    }
    public class ChatResponseDto
    {
        public string? Reply { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Mesaj alanı boş olamaz.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Mesaj en az 1, en fazla 2000 karakter olmalıdır.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "ClientChatSessionId alanı gereklidir.")]
        public string? ClientChatSessionId { get; set; }
    }
    public class ChatResponseDto
    {
        public string? Reply { get; set; }
    }
}
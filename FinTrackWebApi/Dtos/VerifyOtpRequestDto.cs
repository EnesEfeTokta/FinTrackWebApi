using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class InitiateRegistrationDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public string? ProfilePicture { get; set; }
    }
}

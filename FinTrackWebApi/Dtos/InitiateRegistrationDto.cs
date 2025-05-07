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
        public string PasswordHash { get; set; } = null!;

        public string ProfilePicture { get; set; } = "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740";
    }
}

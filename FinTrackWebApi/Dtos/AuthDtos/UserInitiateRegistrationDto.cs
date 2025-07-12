namespace FinTrackWebApi.Dtos.AuthDtos
{
    public class UserInitiateRegistrationDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}

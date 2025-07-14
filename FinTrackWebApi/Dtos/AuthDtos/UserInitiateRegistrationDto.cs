namespace FinTrackWebApi.Dtos.AuthDtos
{
    public class UserInitiateRegistrationDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}

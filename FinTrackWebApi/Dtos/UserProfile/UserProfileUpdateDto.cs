namespace FinTrackWebApi.Dtos.UserProfile
{
    public class UserProfileUpdateDto
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}

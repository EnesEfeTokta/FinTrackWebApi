namespace FinTrackWebApi.Dtos.UserProfile
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string MembershipType { get; set; } = string.Empty;
    }
}

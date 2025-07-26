namespace FinTrackWebApi.Models.Otp
{
    public class OtpVerificationModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
        public string Username { get; set; } = null!;
        public string? ProfilePicture { get; set; } =
            "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740";
        public string TemporaryPlainPassword { get; set; } = null!;
    }
}

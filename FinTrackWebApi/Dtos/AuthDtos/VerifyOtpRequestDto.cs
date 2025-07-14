namespace FinTrackWebApi.Dtos.AuthDtos
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}

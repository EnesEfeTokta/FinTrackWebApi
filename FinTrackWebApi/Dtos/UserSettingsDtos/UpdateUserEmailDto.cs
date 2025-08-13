namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UpdateUserEmailDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}

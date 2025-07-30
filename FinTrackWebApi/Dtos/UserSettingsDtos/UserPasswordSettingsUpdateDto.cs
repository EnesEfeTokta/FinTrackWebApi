namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserPasswordSettingsUpdateDto
    {
        public string Password { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

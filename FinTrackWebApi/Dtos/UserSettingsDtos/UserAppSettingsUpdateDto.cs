using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserAppSettingsUpdateDto
    {
        public AppearanceType Appearance { get; set; }
        public BaseCurrencyType Currency { get; set; }
    }
}

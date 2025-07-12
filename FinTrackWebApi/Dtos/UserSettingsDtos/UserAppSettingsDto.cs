using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserAppSettingsDto
    {
        public int Id { get; set; }
        public AppearanceType Appearance { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class UserAppSettingsModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public AppearanceType? Appearance { get; set; }
        public BaseCurrencyType? BaseCurrency { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

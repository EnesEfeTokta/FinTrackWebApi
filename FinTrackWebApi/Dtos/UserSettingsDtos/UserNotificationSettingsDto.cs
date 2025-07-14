namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserNotificationSettingsDto
    {
        public int Id { get; set; }
        public bool SpendingLimitWarning { get; set; }
        public bool ExpectedBillReminder { get; set; }
        public bool WeeklySpendingSummary { get; set; }
        public bool NewFeaturesAndAnnouncements { get; set; }
        public bool EnableDesktopNotifications { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

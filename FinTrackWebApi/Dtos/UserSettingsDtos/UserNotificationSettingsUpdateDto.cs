namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserNotificationSettingsUpdateDto
    {
        public bool SpendingLimitWarning { get; set; }
        public bool ExpectedBillReminder { get; set; }
        public bool WeeklySpendingSummary { get; set; }
        public bool NewFeaturesAndAnnouncements { get; set; }
        public bool EnableDesktopNotifications { get; set; }
    }
}

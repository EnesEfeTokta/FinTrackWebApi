namespace FinTrackWebApi.Models.User
{
    public class UserNotificationSettingsModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public bool SpendingLimitWarning { get; set; } = true;
        public bool ExpectedBillReminder { get; set; } = true;
        public bool WeeklySpendingSummary { get; set; } = true;
        public bool NewFeaturesAndAnnouncements { get; set; } = true;
        public bool EnableDesktopNotifications { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

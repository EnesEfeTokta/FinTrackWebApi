using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.UserProfile
{
    public class UserProfileDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        // Üyelik Bilgileri
        public int CurrentMembershipPlanId { get; set; }
        public string CurrentMembershipPlanType { get; set; } = string.Empty;
        public DateTime MembershipStartDateUtc { get; set; }
        public DateTime MembershipExpirationDateUtc { get; set; }

        // Ayarlar
        public AppearanceType Thema { get; set; }
        public LanguageType Language { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public bool SpendingLimitWarning { get; set; }
        public bool ExpectedBillReminder { get; set; }
        public bool WeeklySpendingSummary { get; set; }
        public bool NewFeaturesAndAnnouncements { get; set; }
        public bool EnableDesktopNotifications { get; set; }

        // Kullanımlar
        public List<int> CurrentAccounts = new();
        public List<int> CurrentBudgets = new();
        public List<int> CurrentTransactions = new();
        public List<int> CurrentBudgetsCategories = new();
        public List<int> CurrentTransactionsCategories = new();
        public List<int> CurrentLenderDebts = new();
        public List<int> CurrentBorrowerDebts = new();
        public List<int> CurrentNotifications = new();
        public List<int> CurrentFeedbacks = new();
        public List<int> CurrentVideos = new();
    }
}

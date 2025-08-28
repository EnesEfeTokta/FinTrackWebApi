namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserDashboardSettingsDto
    {
        public int Id { get; set; }
        public int[]? SelectedCurrencies { get; set; }
        public int[]? SelectedBudgets { get; set; }
        public int[]? SelectedAccounts { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserDashboardSettingsDto
    {
        public int Id { get; set; }
        public int[]? SelectedCurrencies { get; set; }
        public int[]? SelectedBudgets { get; set; }
        public int[]? SelectedAccounts { get; set; }
    }
}

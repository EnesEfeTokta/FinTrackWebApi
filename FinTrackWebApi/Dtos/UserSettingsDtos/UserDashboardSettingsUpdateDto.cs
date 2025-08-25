namespace FinTrackWebApi.Dtos.UserSettingsDtos
{
    public class UserDashboardSettingsUpdateDto
    {
        public int[]? SelectedCurrencies { get; set; }
        public int[]? SelectedBudgets { get; set; }
        public int[]? SelectedAccounts { get; set; }
    }
}

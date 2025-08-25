namespace FinTrackWebApi.Models.User
{
    public class UserDashboardSettingsModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;

        private int[] _selectedCurrencies = Array.Empty<int>();
        private int[] _selectedBudgets = Array.Empty<int>();
        private int[] _selectedAccounts = Array.Empty<int>();

        public int[] SelectedCurrencies
        {
            get => _selectedCurrencies;
            set
            {
                if (value != null && value.Length > 5)
                {
                    throw new();
                }
                _selectedCurrencies = value ?? Array.Empty<int>();
            }
        }

        public int[] SelectedBudgets
        {
            get => _selectedBudgets;
            set
            {
                if (value != null && value.Length > 4)
                {
                    throw new();
                }
                _selectedBudgets = value ?? Array.Empty<int>();
            }
        }

        public int[] SelectedAccounts
        {
            get => _selectedAccounts;
            set
            {
                if (value != null && value.Length > 2)
                {
                    throw new();
                }
                _selectedAccounts = value ?? Array.Empty<int>();
            }
        }
    }
}

using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.AccountDtos
{
    public class AccountUpdateDto
    {
        public string Name { get; set; } = null!;
        public AccountType Type { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }
        public BaseCurrencyType Currency { get; set; }
    }
}

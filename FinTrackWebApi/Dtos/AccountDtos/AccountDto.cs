using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.AccountDtos
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public AccountType Type { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Account;
using FinTrackWebApi.Models.Tranaction;

namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public TransactionCategoryModel Category { get; set; } = null!;
        public AccountModel Account { get; set; } = null!;
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

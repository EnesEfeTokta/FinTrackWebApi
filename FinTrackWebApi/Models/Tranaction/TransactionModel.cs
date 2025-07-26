using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Account;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Tranaction
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public int CategoryId { get; set; }
        public virtual TransactionCategoryModel Category { get; set; } = null!;
        public int AccountId { get; set; }
        public virtual AccountModel Account { get; set; } = null!;
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

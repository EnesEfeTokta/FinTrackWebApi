using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Tranaction
{
    public class TransactionCategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public AccountType Type { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal Balance { get; set; } = 0;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public virtual ICollection<TransactionModel> Transactions { get; set; } =
            new List<TransactionModel>();
    }
}

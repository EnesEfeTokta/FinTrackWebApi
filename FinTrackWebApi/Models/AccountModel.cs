using Stripe;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Models
{
    public class AccountModel
    {
        [Required]
        [Key]
        public int AccountId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("AccountType")]
        public AccountType Type { get; set; }

        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("Balance")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
        public decimal Balance { get; set; } = 0;

        [Required]
        [DataType(DataType.DateTime)]
        [Column("CreateAt")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Column("UpdateAt")]
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;

        public virtual ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }

    public enum AccountType { Checking, Savings, CreditCard, Cash, Investment, Loan, Other } // Kontrol, Tasarruf, Kredi Kartı, Nakit, Yatırım, Kredi, Diğer
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Transactions")]
    public class TransactionModel
    {
        [Key]
        [Required]
        public int TransactionId { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Required]
        [Column("AccountId")]
        public int AccountId { get; set; }

        [Required]
        [Column("Amount")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("TransactionDate")]
        public DateTime TransactionDateUtc { get; set; }

        [Required]
        [Column("Description")]
        public string Description { get; set; } = null!;

        [Required]
        [Column("CreatedAt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual CategoryModel Category { get; set; } = null!;

        [ForeignKey("AccountId")]
        public virtual AccountModel Account { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Categories")]
    public class CategoryModel
    {
        [Key]
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [Column("CategoryName")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("CategoryType")]
        public CategoryType Type { get; set; }

        public virtual UserModel User { get; set; } = null!;
        public virtual ICollection<BudgetCategoryModel> BudgetAllocations { get; set; } = new List<BudgetCategoryModel>();
        public virtual ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }

    public enum CategoryType
    {
        Expense = 0,
        Income = 1
    }
}
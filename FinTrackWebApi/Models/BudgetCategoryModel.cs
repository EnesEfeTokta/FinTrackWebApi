using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("BudgetCategories")]
    public class BudgetCategoryModel
    {
        [Key]
        [Required]
        public int BudgetCategoryId { get; set; }

        [Required]
        [ForeignKey("Budget")]
        public int BudgetId { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        [Column("AllocatedAmount", TypeName = "decimal(18, 2)")]
        [Range(0.00, (double)decimal.MaxValue)]
        public decimal AllocatedAmount { get; set; }

        public virtual BudgetModel Budget { get; set; } = null!;
        public virtual CategoryModel Category { get; set; } = null!;
    }
}

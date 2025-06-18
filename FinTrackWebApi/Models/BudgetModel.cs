using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Budgets")]
    public class BudgetModel
    {
        [Key]
        [Required]
        public int BudgetId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [Column("BudgetName")]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column("EndDate")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CreatedAtUtc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAtUtc")]
        public DateTime? UpdatedAtUtc { get; set; }

        public virtual UserModel User { get; set; } = null!;
        public virtual ICollection<BudgetCategoryModel> BudgetCategories { get; set; } =
            new List<BudgetCategoryModel>();
    }
}

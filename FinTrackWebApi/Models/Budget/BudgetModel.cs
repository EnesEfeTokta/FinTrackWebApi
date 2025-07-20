using FinTrackWebApi.Models.User;
using System.ComponentModel.DataAnnotations.Schema;
using FinTrackWebApi.Models.Category;
using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models.Budget
{
    [Table("Budgets")]
    public class BudgetModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public int CategoryId { get; set; }
        public virtual CategoryModel Category { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal AllocatedAmount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

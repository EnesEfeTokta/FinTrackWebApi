using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class BudgetCategoryModel
    {
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public virtual BudgetModel Budget { get; set; } = null!;
        public int CategoryId { get; set; }
        public virtual CategoryModel Category { get; set; } = null!;
        public decimal AllocatedAmount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

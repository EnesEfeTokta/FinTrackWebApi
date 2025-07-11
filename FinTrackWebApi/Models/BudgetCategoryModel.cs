namespace FinTrackWebApi.Models
{
    public class BudgetCategoryModel
    {
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public decimal AllocatedAmount { get; set; }

        public virtual BudgetModel Budget { get; set; } = null!;
        public virtual CategoryModel Category { get; set; } = null!;
    }
}

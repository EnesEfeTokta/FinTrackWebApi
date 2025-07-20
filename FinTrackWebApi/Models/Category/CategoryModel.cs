using FinTrackWebApi.Models.Budget;
using FinTrackWebApi.Models.Tranaction;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Category
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<BudgetModel> Budgets { get; set; } = new List<BudgetModel>();
    }
}

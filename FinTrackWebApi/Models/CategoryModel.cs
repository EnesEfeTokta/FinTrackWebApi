namespace FinTrackWebApi.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public virtual ICollection<BudgetCategoryModel> BudgetAllocations { get; set; } =
            new List<BudgetCategoryModel>();
        public virtual ICollection<TransactionModel> Transactions { get; set; } =
            new List<TransactionModel>();
    }
}

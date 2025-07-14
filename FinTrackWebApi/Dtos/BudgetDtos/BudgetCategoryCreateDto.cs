using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.BudgetDtos
{
    public class BudgetCategoryCreateDto
    {
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public BaseCurrencyType Currency { get; set; }
    }
}

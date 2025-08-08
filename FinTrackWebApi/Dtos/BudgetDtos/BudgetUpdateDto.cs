using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.BudgetDtos
{
    public class BudgetUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Category { get; set; } = null!;
        public decimal AllocatedAmount { get; set; }
        public decimal? ReachedAmount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Membership;

namespace FinTrackWebApi.Dtos.MembershipPlansDtos
{
    public class PlanFeatureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public BaseCurrencyType? Currency { get; set; }
        public BillingCycleType? BillingCycle { get; set; }
        public int? DurationInDays { get; set; }
        public ReportingFeatures? Reporting { get; set; }
        public EmailingFeatures? Emailing { get; set; }
        public BudgetingFeatures? Budgeting { get; set; }
        public AccountFeatures? Accounts { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool PrioritySupport { get; set; } = false;
    }
}

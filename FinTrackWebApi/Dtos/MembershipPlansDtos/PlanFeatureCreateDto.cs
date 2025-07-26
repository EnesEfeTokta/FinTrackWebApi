using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Membership;

namespace FinTrackWebApi.Dtos.MembershipPlansDtos
{
    public class PlanFeatureCreateDto
    {
        public string PlanName { get; set; } = null!;
        public string PlanDescription { get; set; } = null!;
        public decimal Price { get; set; }
        public int? DurationInDays { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public BillingCycleType BillingCycle { get; set; }
        public ReportingFeatures? Reporting { get; set; }
        public EmailingFeatures? Emailing { get; set; }
        public BudgetingFeatures? Budgeting { get; set; }
        public AccountFeatures? Accounts { get; set; }
        public bool IsActive { get; set; } = false;
        public bool PrioritySupport { get; set; } = false;
    }
}

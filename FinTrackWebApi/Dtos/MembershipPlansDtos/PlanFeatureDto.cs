using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.MembershipPlansDtos
{
    public class PlanFeatureDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = null!;
        public string PlanDescription { get; set; } = null!;
        public decimal Price { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public ReportingFeatures Reporting { get; set; } = new();
        public EmailingFeatures Emailing { get; set; } = new();
        public BudgetingFeatures Budgeting { get; set; } = new();
        public AccountFeatures Accounts { get; set; } = new();
    }
}

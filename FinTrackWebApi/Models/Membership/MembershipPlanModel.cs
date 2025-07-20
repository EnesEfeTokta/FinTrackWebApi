using FinTrackWebApi.Dtos.MembershipPlansDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Membership
{
    public class MembershipPlanModel
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool PrioritySupport { get; set; } = false;

        public virtual ICollection<UserMembershipModel> UserMemberships { get; set; } =
            new List<UserMembershipModel>();
    }

    public class ReportingFeatures
    {
        public string Level { get; set; } = "Basic";
        public bool CanExportPdf { get; set; } = false;
        public bool CanExportWord { get; set; } = false;
        public bool CanExportMarkdown { get; set; } = false;
        public bool CanExportXml { get; set; } = false;
        public bool CanExportText { get; set; } = false;
        public bool CanExportXlsx { get; set; } = false;
    }

    public class EmailingFeatures
    {
        public bool CanEmailReports { get; set; } = false;
        public int MaxEmailsPerMonth { get; set; } = 0;
    }

    public class BudgetingFeatures
    {
        public bool CanCreateBudgets { get; set; } = false;
        public int MaxBudgets { get; set; } = 0;
    }

    public class AccountFeatures
    {
        public int MaxBankAccounts { get; set; } = 0;
    }
}

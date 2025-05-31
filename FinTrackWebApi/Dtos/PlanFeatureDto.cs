namespace FinTrackWebApi.Dtos
{
    public class PlanFeatureSetDto
    {
        public ReportingFeatures Reporting { get; set; } = new();
        public EmailingFeatures Emailing { get; set; } = new();
        public BudgetingFeatures Budgeting { get; set; } = new();
        public AccountFeatures Accounts { get; set; } = new();

        public bool PrioritySupport { get; set; } = false;
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

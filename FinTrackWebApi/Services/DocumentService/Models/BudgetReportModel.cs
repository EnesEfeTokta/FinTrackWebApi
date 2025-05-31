using FinTrackWebApi.Services.DocumentService.Generations;

namespace FinTrackWebApi.Services.DocumentService.Models
{
    public class BudgetReportModel : IReportModel
    {
        public string ReportTitle { get; set; } = "Default Report";
        public string Description { get; set; } = string.Empty;
        public List<BudgetReportTableItem> Items { get; set; } = new List<BudgetReportTableItem>();
    }

    public class BudgetReportTableItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal AllocatedAmount { get; set; }
    }
}

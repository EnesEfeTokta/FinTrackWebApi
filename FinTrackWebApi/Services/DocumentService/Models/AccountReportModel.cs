using FinTrackWebApi.Enums;
using FinTrackWebApi.Services.DocumentService.Generations;
using System.Collections;

namespace FinTrackWebApi.Services.DocumentService.Models
{
    public class AccountReportModel : IReportModel
    {
        public string ReportTitle { get; set; } = "Default Report";
        public string Description { get; set; } = string.Empty;
        public List<AccountReportTableItem> Items { get; set; } = new List<AccountReportTableItem>();

        IList IReportModel.Items => this.Items;

        public int AccountCount { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class AccountReportTableItem
    {
        public string Name { get; set; } = string.Empty;
        public AccountType? Type { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

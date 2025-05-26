using FinTrackWebApi.Models;

namespace FinTrackWebApi.Services.DocumentService.Models
{
    public class TransactionsRaportModel
    {
        public string ReportTitle { get; set; } = "Default Report";
        public string Description { get; set; } = string.Empty;
        public List<TransactionRaportTableItem> Items { get; set; } = new List<TransactionRaportTableItem>();
        public decimal TotalCount { get; set; }
    }

    public class TransactionRaportTableItem
    {
        public string AccountName { get; set; } = String.Empty;
        public CategoryType Category { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDateUtc { get; set; }
    }
}

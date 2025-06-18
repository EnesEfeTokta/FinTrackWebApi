using FinTrackWebApi.Services.DocumentService.Generations;

namespace FinTrackWebApi.Services.DocumentService.Models
{
    public class TransactionsRaportModel : IReportModel
    {
        public string ReportTitle { get; set; } = "Default Report";
        public string Description { get; set; } = string.Empty;
        public List<TransactionRaportTableItem> Items { get; set; } = new List<TransactionRaportTableItem>();
        public decimal TotalCount { get; set; }
    }

    public class TransactionRaportTableItem
    {
        public string AccountName { get; set; } = String.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDateUtc { get; set; }
    }
}

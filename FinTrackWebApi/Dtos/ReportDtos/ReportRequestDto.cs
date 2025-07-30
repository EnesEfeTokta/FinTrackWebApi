using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.ReportDtos
{
    public class ReportRequestDto
    {
        public ReportType ReportType { get; set; }
        public Enums.DocumentFormat ExportFormat { get; set; }

        // Genel Filtreler
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? SelectedCategoryIds { get; set; }
        public List<int>? SelectedAccountIds { get; set; }

        // Bütçe Raporuna Özel
        public List<int>? SelectedBudgetIds { get; set; }

        // Hesap Raporuna Özel
        public decimal? MinBalance { get; set; }
        public decimal? MaxBalance { get; set; }

        // İşlem Raporuna Özel
        public DateTime? Date { get; set; }
        public bool IsIncomeSelected { get; set; }
        public bool IsExpenseSelected { get; set; }
        public string? SelectedSortingCriterion { get; set; } // Örn: "Amount_Asc", "Date_Desc"
    }
}

namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class CurrencyHistoryDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public string TargetCurrency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ChangeSummaryDto ChangeSummary { get; set; } = new();

        public List<HistoricalRatePointDto> HistoricalRates { get; set; } = new();
    }
}

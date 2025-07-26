namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class ChangeSummaryDto
    {
        // Değişim Miktarı
        public decimal? DailyChangeValue { get; set; }
        public decimal? WeeklyChangeValue { get; set; }
        public decimal? MonthlyChangeValue { get; set; }

        // Değişim Oranı
        public decimal? DailyChangePercentage { get; set; }
        public decimal? WeeklyChangePercentage { get; set; }
        public decimal? MonthlyChangePercentage { get; set; }
    }
}

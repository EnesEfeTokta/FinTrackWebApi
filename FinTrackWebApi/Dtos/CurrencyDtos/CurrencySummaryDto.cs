namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class CurrencySummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }
}

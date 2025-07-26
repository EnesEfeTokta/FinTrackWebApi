namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class SpecificCurrencyDto
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Status { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public string? IconUrl { get; set; }

        public CalculatedRatesDto? RatesInfo { get; set; }
    }
}

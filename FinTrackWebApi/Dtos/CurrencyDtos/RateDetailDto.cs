namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class RateDetailDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public string? IconUrl { get; set; }

        public decimal Rate { get; set; }
    }
}

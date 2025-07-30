namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class LatestRatesResponseDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public DateTime SnapshotTimestamp { get; set; }
        public List<RateDetailDto> Rates { get; set; } = new();
    }
}

namespace FinTrackWebApi.Dtos.CurrencyDtos
{
    public class CalculatedRatesDto
    {
        public DateTime SnapshotTimestamp { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public Dictionary<string, decimal> CrossRates { get; set; } = new();
    }
}

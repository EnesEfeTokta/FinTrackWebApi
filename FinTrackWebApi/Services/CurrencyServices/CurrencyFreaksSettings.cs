namespace FinTrackWebApi.Services.CurrencyServices
{
    public class CurrencyFreaksSettings
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? SupportedCurrenciesUrl { get; set; }
        public int UpdateIntervalMinutes { get; set; } = 10;
    }
}

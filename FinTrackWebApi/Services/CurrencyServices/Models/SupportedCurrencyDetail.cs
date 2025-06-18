using System.Text.Json.Serialization;

namespace FinTrackWebApi.Services.CurrencyServices.Models
{
    public class SupportedCurrencyDetail
    {
        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("currencyName")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("countryName")]
        public string? CountryName { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("availableFrom")]
        public string? AvailableFromString { get; set; }

        [JsonPropertyName("availableUntil")]
        public string? AvailableUntilString { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
    }

    public class SupportedCurrenciesApiResponse
    {
        [JsonPropertyName("supportedCurrenciesMap")]
        public Dictionary<string, SupportedCurrencyDetail>? SupportedCurrenciesMap { get; set; }
    }
}

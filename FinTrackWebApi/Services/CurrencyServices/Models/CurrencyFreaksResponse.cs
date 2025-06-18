using System.Text.Json.Serialization;
using FinTrackWebApi.Utils;

namespace FinTrackWebApi.Services.CurrencyServices.Models
{
    public class CurrencyFreaksResponse
    {
        [JsonPropertyName("date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonPropertyName("base")]
        public string? Base { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}

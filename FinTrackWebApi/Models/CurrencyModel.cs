using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class CurrencyModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Status { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public string? IconUrl { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public virtual ICollection<ExchangeRateModel> ExchangeRates { get; set; } =
            new List<ExchangeRateModel>();
    }
}

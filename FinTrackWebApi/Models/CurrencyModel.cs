using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Currencies")]
    public class CurrencyModel
    {
        [Key]
        public int CurrencyId { get; set; }

        [Required]
        [Column("Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("CountryCode")]
        public string? CountryCode { get; set; }

        [Required]
        [Column("CountryName")]
        public string? CountryName { get; set; }

        [Required]
        [Column("Status")]
        public string? Status { get; set; }

        [Column("AvailableFrom")]
        [DataType(DataType.Date)]
        public DateTime? AvailableFrom { get; set; }

        [Column("AvailableUntil")]
        [DataType(DataType.Date)]
        public DateTime? AvailableUntil { get; set; }

        [Column("IconUrl")]
        public string? IconUrl { get; set; }

        [Required]
        [Column("CreatedUtc")]
        [DataType(DataType.DateTime)]
        public DateTime LastUpdatedUtc { get; set; }

        public virtual ICollection<ExchangeRateModel> ExchangeRates { get; set; } = new List<ExchangeRateModel>();
    }
}

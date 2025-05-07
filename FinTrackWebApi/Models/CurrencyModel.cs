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
        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string Code { get; set; } = string.Empty;

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Name { get; set; }

        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string? CountryCode { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? CountryName { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? Status { get; set; }

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableUntil { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? IconUrl { get; set; }

        public DateTime LastUpdatedUtc { get; set; }

        public virtual ICollection<ExchangeRateModel> ExchangeRates { get; set; } = new List<ExchangeRateModel>();
    }
}

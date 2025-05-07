using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("ExchangeRates")]
    public class ExchangeRateModel
    {
        [Required]
        [Key]
        public int ExchangeRateId { get; set; }

        [Required]
        [ForeignKey("CurrencyId")]
        public int CurrencyId { get; set; }

        public virtual CurrencyModel? Currency { get; set; } = null;

        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Rate { get; set; }

        [Required]
        [ForeignKey("CurrencySnapshotId ")]
        public int CurrencySnapshotId { get; set; }

        public virtual CurrencySnapshotModel CurrencySnapshot { get; set; } = null!;
    }
}

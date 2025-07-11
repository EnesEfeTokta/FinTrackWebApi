using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class ExchangeRateModel
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public virtual CurrencyModel? Currency { get; set; } = null;
        public int CurrencySnapshotId { get; set; }
        public virtual CurrencySnapshotModel CurrencySnapshot { get; set; } = null!;
        public decimal Rate { get; set; }
    }
}

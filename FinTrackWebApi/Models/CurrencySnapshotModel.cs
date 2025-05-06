using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class CurrencySnapshotModel
    {
        [Key]
        [Required]
        public int CurrencySnapshotId { get; set; }

        [Required]
        [Column("FetchTimestamp")]
        [DataType(DataType.DateTime)]
        public DateTime FetchTimestamp { get; set; }

        [Required]
        [Column("BaseCurrency")]
        public string BaseCurrency { get; set; } = string.Empty;

        public virtual ICollection<ExchangeRateModel> Rates { get; set; } = new List<ExchangeRateModel>();
    }
}

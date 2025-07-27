namespace FinTrackWebApi.Models.Currency
{
    public class CurrencySnapshotModel
    {
        public int Id { get; set; }
        public DateTime FetchTimestamp { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public bool HasChanges { get; set; }

        public virtual ICollection<ExchangeRateModel> Rates { get; set; } =
            new List<ExchangeRateModel>();
    }
}

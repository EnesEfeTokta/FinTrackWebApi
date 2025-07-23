using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionCreateDto
    {
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public string? Description { get; set; }
    }
}

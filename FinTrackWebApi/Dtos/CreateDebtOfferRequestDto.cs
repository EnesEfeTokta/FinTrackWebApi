namespace FinTrackWebApi.Dtos
{
    public class CreateDebtOfferRequestDto
    {
        public string LenderId { get; set; } = string.Empty;

        public string BorrowerId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public DateTime DueDateUtc { get; set; }

        public string? Description { get; set; }
    }
}

namespace FinTrackWebApi.Dtos
{
    public class CreateDebtOfferRequestDto
    {
        public int BorrowerId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public DateTime DueDateUtc { get; set; }

        public string? Description { get; set; }
    }
}

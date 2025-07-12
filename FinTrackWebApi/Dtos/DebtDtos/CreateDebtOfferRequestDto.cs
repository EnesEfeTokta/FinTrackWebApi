using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.DebtDtos
{
    public class CreateDebtOfferRequestDto
    {
        public string BorrowerEmail { get; set; } = null!;

        public decimal Amount { get; set; }

        public BaseCurrencyType CurrencyCode { get; set; }

        public DateTime DueDateUtc { get; set; }

        public string? Description { get; set; }
    }
}

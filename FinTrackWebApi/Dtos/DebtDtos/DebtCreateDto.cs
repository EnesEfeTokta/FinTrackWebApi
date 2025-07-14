using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.DebtDtos
{
    public class DebtCreateDto
    {
        public string BorrowerEmail { get; set; } = null!;
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

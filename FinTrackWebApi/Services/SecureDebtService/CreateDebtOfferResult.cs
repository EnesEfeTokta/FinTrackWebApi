using FinTrackWebApi.Models.Debt;

namespace FinTrackWebApi.Services.SecureDebtService
{
    public class CreateDebtOfferResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DebtModel? CreatedDebt { get; set; }
        public int? DebtId => CreatedDebt?.Id;
    }
}

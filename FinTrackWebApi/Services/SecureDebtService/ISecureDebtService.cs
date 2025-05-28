using FinTrackWebApi.Models;

namespace FinTrackWebApi.Services.SecureDebtService
{
    public interface ISecureDebtService
    {
        Task<CreateDebtOfferResult> CreateDebtOfferAsync(
            string lenderUserId,
            string borrowerEmail,
            decimal amount,
            CurrencyModel currency,
            DateTime dueDate,
            string? description);
    }
}

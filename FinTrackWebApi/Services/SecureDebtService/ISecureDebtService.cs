using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Services.SecureDebtService
{
    public interface ISecureDebtService
    {
        Task<CreateDebtOfferResult> CreateDebtOfferAsync(
            UserModel lender,
            UserModel borrower,
            decimal amount,
            BaseCurrencyType currency,
            DateTime dueDate,
            string? description
        );
    }
}

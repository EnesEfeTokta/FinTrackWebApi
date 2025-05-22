using FinTrackWebApi.Services.CurrencyServices.Models;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public interface ICurrencyFreaksProvider
    {
        Task<CurrencyFreaksResponse?> GetLatestRatesAsync(CancellationToken cancellationToken);
    }
}
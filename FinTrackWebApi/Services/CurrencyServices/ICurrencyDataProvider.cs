using FinTrackWebApi.Services.CurrencyServices.Models;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public interface ICurrencyDataProvider
    {
        Task<CurrencyFreaksResponse?> GetLatestRatesAsync(CancellationToken cancellationToken);
    }
}
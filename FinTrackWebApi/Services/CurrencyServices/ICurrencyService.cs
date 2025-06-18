using FinTrackWebApi.Services.CurrencyServices.Models;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public interface ICurrencyService
    {
        CurrencyFreaksResponse? GetLatestRatesFromCache();
        decimal? GetSpecificRateFromCache(string targetCurrency);
    }
}

using FinTrackWebApi.Services.CurrencyServices.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public class CurrencyCacheService : ICurrencyService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyCacheService> _logger;

        public CurrencyCacheService(IMemoryCache cache, ILogger<CurrencyCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public CurrencyFreaksResponse? GetLatestRatesFromCache()
        {
            if (_cache.TryGetValue(CacheKeys.LatestRates, out CurrencyFreaksResponse? latestRates))
            {
                return latestRates;
            }
            return null;
        }

        public decimal? GetSpecificRateFromCache(string targetCurrency)
        {
            var rates = GetLatestRatesFromCache();
            targetCurrency = targetCurrency.ToUpperInvariant();

            if (
                rates != null
                && rates.Rates != null
                && rates.Rates.TryGetValue(targetCurrency, out decimal rate)
            )
            {
                _logger.LogInformation(
                    "Rate for {TargetCurrency} found in cache: {Rate}",
                    targetCurrency,
                    rate
                );
                return rate;
            }

            _logger.LogWarning("Rate for {TargetCurrency} not found in cache.", targetCurrency);
            return null;
        }
    }
}

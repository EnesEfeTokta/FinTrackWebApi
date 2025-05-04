using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public class CurrencyUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CurrencyUpdateService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _updateInterval;
        private readonly CurrencyFreaksSettings _settings;

        public CurrencyUpdateService(
            IServiceProvider serviceProvider,
            ILogger<CurrencyUpdateService> logger,
            IMemoryCache cache,
            IOptions<CurrencyFreaksSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = cache;
            _settings = settings.Value;

            _updateInterval = TimeSpan.FromMinutes(_settings.UpdateIntervalMinutes);

            _logger.LogInformation("CurrencyUpdateService başlatıldı. Güncelleme aralığı: {Interval}", _updateInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    await DoWorkWithScopeAsync(stoppingToken);
                }
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "An error occurred during the initial currency update.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Waiting until {Interval} for the next rate update.", _updateInterval);
                    await Task.Delay(_updateInterval, stoppingToken);
                    await DoWorkWithScopeAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the rate update cycle.");
                }
            }
            _logger.LogInformation("Stopping the Currency Update Service ExecuteAsync.");
        }

        private async Task DoWorkWithScopeAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var scopedDataProvider = scope.ServiceProvider.GetRequiredService<ICurrencyDataProvider>();
                    await FetchAndUpdateRatesAsync(scopedDataProvider, stoppingToken);

                    _logger.LogInformation("The scope of the currency update work has been completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in DoWorkWithScopeAsync.");
                }
            }
        }

        private async Task FetchAndUpdateRatesAsync(ICurrencyDataProvider dataProvider, CancellationToken stoppingToken)
        {
            try
            {
                var ratesResponse = await dataProvider.GetLatestRatesAsync(stoppingToken);

                if (ratesResponse != null && ratesResponse.Rates != null && ratesResponse.Rates.Any())
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_updateInterval.Add(TimeSpan.FromMinutes(5)));

                    _logger.LogInformation("Successfully withdrawn {Count}. Caching Base: {Base}, Date: {Date}",
                        ratesResponse.Rates.Count, ratesResponse.Base, ratesResponse.Date);
                    _cache.Set(CacheKeys.LatestRates, ratesResponse, cacheEntryOptions);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve valid exchange rate data from CurrencyFreaks API. The cache was not updated.");
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "An error occurred during FetchAndUpdateRatesAsync."); 
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Currency Update Service StopAsync has been called.");

            await base.StopAsync(stoppingToken);
        }
    }
}
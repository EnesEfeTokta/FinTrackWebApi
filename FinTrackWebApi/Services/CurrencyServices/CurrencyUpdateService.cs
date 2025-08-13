using FinTrackWebApi.Data;
using FinTrackWebApi.Models.Currency;
using FinTrackWebApi.Services.CurrencyServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

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
            _logger.LogInformation("CurrencyUpdateService has been started. Update interval: {Interval}", _updateInterval);
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
                    await Task.Delay(_updateInterval, stoppingToken);
                    if (stoppingToken.IsCancellationRequested) break;
                    await DoWorkWithScopeAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("The rate update cycle was canceled because the application is shutting down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred in the rate update cycle.");
                }
            }
        }

        private async Task DoWorkWithScopeAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var scopedServiceProvider = scope.ServiceProvider;
                    var dataProvider = scopedServiceProvider.GetRequiredService<ICurrencyDataProvider>();
                    var dbContext = scopedServiceProvider.GetRequiredService<MyDataContext>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    var ratesResponse = await dataProvider.GetLatestRatesAsync(stoppingToken);

                    await UpdateSupportedCurrenciesAsync(dbContext, httpClientFactory, stoppingToken);

                    if (ratesResponse != null && ratesResponse.Rates != null && ratesResponse.Rates.Any())
                    {
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(_updateInterval.Add(TimeSpan.FromMinutes(5)));
                        _cache.Set(CacheKeys.LatestRates, ratesResponse, cacheEntryOptions);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retrieve valid exchange rate data from CurrencyFreaks API. The cache was not updated.");
                    }

                    await SaveRatesToDatabaseAsync(dbContext, ratesResponse, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in DoWorkWithScopeAsync.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Currency Update Service StopAsync has been called.");
            await base.StopAsync(stoppingToken);
        }

        private async Task SaveRatesToDatabaseAsync(
            MyDataContext dbContext,
            CurrencyFreaksResponse? newRatesResponse,
            CancellationToken cancellationToken)
        {
            if (newRatesResponse?.Rates == null || !newRatesResponse.Rates.Any())
            {
                _logger.LogWarning("Snapshot is not created because no rates were received from the API.");
                return;
            }

            try
            {
                DateTime fetchTimestampUtc = DateTime.SpecifyKind(newRatesResponse.Date, DateTimeKind.Utc);

                var existingSnapshot = await dbContext.CurrencySnapshots
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cs => cs.FetchTimestamp == fetchTimestampUtc, cancellationToken);

                if (existingSnapshot != null)
                {
                    _logger.LogInformation("A snapshot for this timestamp ({Timestamp}) already exists. The operation is skipped.", fetchTimestampUtc);
                    return;
                }

                var dbCurrenciesMap = await dbContext
                    .Currencies.AsNoTracking()
                    .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

                var lastSnapshotRates = await dbContext
                    .ExchangeRates
                    .Where(er => er.CurrencySnapshot.BaseCurrency == newRatesResponse.Base)
                    .Include(er => er.Currency)
                    .OrderByDescending(er => er.CurrencySnapshot.FetchTimestamp)
                    .GroupBy(er => er.Currency.Code)
                    .Select(g => g.First())
                    .ToDictionaryAsync(er => er.Currency.Code, er => er.Rate, cancellationToken);

                var newDbSnapshot = new CurrencySnapshotModel
                {
                    FetchTimestamp = fetchTimestampUtc,
                    BaseCurrency = newRatesResponse.Base,
                    HasChanges = false,
                    Rates = new List<ExchangeRateModel>()
                };

                const int DefaultRatePrecision = 6;
                bool anyRateChanged = false;

                foreach (var apiRatePair in newRatesResponse.Rates)
                {
                    string currencyCodeFromApi = apiRatePair.Key;
                    if (!dbCurrenciesMap.TryGetValue(currencyCodeFromApi, out int currencyIdInDb))
                    {
                        continue;
                    }

                    decimal processedApiRate = Math.Round(apiRatePair.Value, DefaultRatePrecision, MidpointRounding.AwayFromZero);
                    bool rateIsNewOrChanged = true;

                    if (lastSnapshotRates.TryGetValue(currencyCodeFromApi, out decimal lastRateInDb))
                    {
                        decimal processedDbRate = Math.Round(lastRateInDb, DefaultRatePrecision, MidpointRounding.AwayFromZero);
                        if (processedDbRate == processedApiRate)
                        {
                            rateIsNewOrChanged = false;
                        }
                    }

                    if (rateIsNewOrChanged)
                    {
                        anyRateChanged = true;
                        newDbSnapshot.Rates.Add(new ExchangeRateModel
                        {
                            CurrencyId = currencyIdInDb,
                            Rate = processedApiRate
                        });
                    }
                }

                newDbSnapshot.HasChanges = anyRateChanged;

                dbContext.CurrencySnapshots.Add(newDbSnapshot);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (newDbSnapshot.HasChanges)
                {
                    _logger.LogInformation(
                        "A new snapshot containing changes (ID: {SnapshotId}) has been saved with {RateCount} rates.",
                        newDbSnapshot.Id, newDbSnapshot.Rates.Count
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "A new control snapshot without changes (ID: {SnapshotId}) has been saved.",
                        newDbSnapshot.Id
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the snapshot.");
            }
        }

        private async Task UpdateSupportedCurrenciesAsync(
            MyDataContext dbContext,
            IHttpClientFactory httpClientFactory,
            CancellationToken stoppingToken)
        {
            try
            {
                var client = httpClientFactory.CreateClient("CurrencyFreaksClient");

                if (string.IsNullOrWhiteSpace(_settings.SupportedCurrenciesUrl))
                {
                    _logger.LogError("SupportedCurrenciesUrl is not configured.");
                    return;
                }

                var requestUri = _settings.SupportedCurrenciesUrl;
                var response = await client.GetAsync(requestUri, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync(stoppingToken);
                    var apiResponse = JsonSerializer.Deserialize<SupportedCurrenciesApiResponse>(
                        jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.SupportedCurrenciesMap != null && apiResponse.SupportedCurrenciesMap.Any())
                    {
                        int newCurrenciesCount = 0;
                        int updatedCurrenciesCount = 0;

                        var existingDbCurrencies = await dbContext.Currencies.ToDictionaryAsync(c => c.Code, c => c, stoppingToken);

                        foreach (var apiKeyValuePair in apiResponse.SupportedCurrenciesMap)
                        {
                            string apiCurrencyCode = apiKeyValuePair.Key;
                            SupportedCurrencyDetail apiDetail = apiKeyValuePair.Value;

                            if (string.IsNullOrWhiteSpace(apiCurrencyCode) || string.IsNullOrWhiteSpace(apiDetail.CurrencyName))
                            {
                                continue;
                            }

                            DateTime? availableFrom = null;
                            if (!string.IsNullOrWhiteSpace(apiDetail.AvailableFromString) && DateTime.TryParse(apiDetail.AvailableFromString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDate))
                            {
                                availableFrom = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                            }

                            DateTime? availableUntil = null;
                            if (!string.IsNullOrWhiteSpace(apiDetail.AvailableUntilString) && DateTime.TryParse(apiDetail.AvailableUntilString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime untilDate))
                            {
                                availableUntil = DateTime.SpecifyKind(untilDate, DateTimeKind.Utc);
                            }

                            if (existingDbCurrencies.TryGetValue(apiCurrencyCode, out CurrencyModel? dbCurrency))
                            {
                                bool changed = false;
                                if (dbCurrency.Name != apiDetail.CurrencyName) { dbCurrency.Name = apiDetail.CurrencyName; changed = true; }
                                if (dbCurrency.CountryCode != apiDetail.CountryCode) { dbCurrency.CountryCode = apiDetail.CountryCode; changed = true; }
                                if (dbCurrency.CountryName != apiDetail.CountryName) { dbCurrency.CountryName = apiDetail.CountryName; changed = true; }
                                if (dbCurrency.Status != apiDetail.Status) { dbCurrency.Status = apiDetail.Status; changed = true; }
                                if (dbCurrency.AvailableFrom != availableFrom) { dbCurrency.AvailableFrom = availableFrom; changed = true; }
                                if (dbCurrency.AvailableUntil != availableUntil) { dbCurrency.AvailableUntil = availableUntil; changed = true; }
                                if (dbCurrency.IconUrl != apiDetail.Icon) { dbCurrency.IconUrl = apiDetail.Icon; changed = true; }

                                if (changed)
                                {
                                    dbCurrency.LastUpdatedUtc = DateTime.UtcNow;
                                    dbContext.Currencies.Update(dbCurrency);
                                    updatedCurrenciesCount++;
                                }
                            }
                            else
                            {
                                var newCurrency = new CurrencyModel
                                {
                                    Code = apiCurrencyCode,
                                    Name = apiDetail.CurrencyName,
                                    CountryCode = apiDetail.CountryCode,
                                    CountryName = apiDetail.CountryName,
                                    Status = apiDetail.Status,
                                    AvailableFrom = availableFrom,
                                    AvailableUntil = availableUntil,
                                    IconUrl = apiDetail.Icon,
                                    LastUpdatedUtc = DateTime.UtcNow,
                                };
                                await dbContext.Currencies.AddAsync(newCurrency, stoppingToken);
                                newCurrenciesCount++;
                            }

                        }

                        if (newCurrenciesCount > 0 || updatedCurrenciesCount > 0)
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogError("API error when withdrawing supported currencies. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating supported currencies.");
            }
        }
    }
}
using FinTrackWebApi.Data;
using FinTrackWebApi.Services.CurrencyServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;
using DocumentFormat.OpenXml.VariantTypes;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

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
                    var scopedDataProvider = scope.ServiceProvider;

                    _logger.LogInformation("The scope of the currency update work has been completed.");

                    var dataProvider = scopedDataProvider.GetRequiredService<ICurrencyDataProvider>();
                    var dbContext = scopedDataProvider.GetRequiredService<MyDataContext>();

                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    CurrencyFreaksResponse? ratesResponse = null;

                    try
                    {
                        ratesResponse = await dataProvider.GetLatestRatesAsync(stoppingToken);

                        await UpdateSupportedCurrenciesAsync(dbContext, httpClientFactory, stoppingToken);

                        if (ratesResponse != null && ratesResponse.Rates != null && ratesResponse.Rates.Any())
                        {
                            try
                            {
                                var cacheEntryOptions = new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(_updateInterval.Add(TimeSpan.FromMinutes(5)));
                                _cache.Set(CacheKeys.LatestRates, ratesResponse, cacheEntryOptions);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred while caching the latest rates.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to retrieve valid exchange rate data from CurrencyFreaks API. The cache was not updated.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while fetching the latest rates.");
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

        private const int DefaultRatePrecision = 6; 

        private async Task SaveRatesToDatabaseAsync(MyDataContext dbContext, CurrencyFreaksResponse? newRatesResponse, CancellationToken cancellationToken)
        {
            try
            {
                var dbCurrenciesMap = await dbContext.Currencies
                                                     .AsNoTracking()
                                                     .ToDictionaryAsync(c => c.Code, c => c.CurrencyId, cancellationToken);

                // API'den gelen ve DB'de olmayan yeni para birimlerini tespit et ve ekle.
                var newCurrencyCodesToAdd = new List<string>();
                foreach (var apiCurrencyCode in newRatesResponse.Rates.Keys)
                {
                    if (!dbCurrenciesMap.ContainsKey(apiCurrencyCode))
                    {
                        newCurrencyCodesToAdd.Add(apiCurrencyCode);
                    }
                }

                if (newCurrencyCodesToAdd.Any())
                {
                    _logger.LogInformation("{Count} of new currencies detected: {Codes}. They are being added to the database.",
                        newCurrencyCodesToAdd.Count, string.Join(", ", newCurrencyCodesToAdd));

                    foreach (var code in newCurrencyCodesToAdd)
                    {
                        dbContext.Currencies.Add(new CurrencyModel { Code = code, Name = $"Currency {code}", LastUpdatedUtc = DateTime.UtcNow });
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("New currencies were registered in the database.");

                    var newlyAddedCurrencies = await dbContext.Currencies
                                                              .Where(c => newCurrencyCodesToAdd.Contains(c.Code))
                                                              .ToDictionaryAsync(c => c.Code, c => c.CurrencyId, cancellationToken);
                    foreach (var addedCurrency in newlyAddedCurrencies)
                    {
                        dbCurrenciesMap[addedCurrency.Key] = addedCurrency.Value;
                    }

                    _logger.LogDebug("The currency map has been updated with new additions.");
                }

                var lastSnapshotRates = await dbContext.ExchangeRates
                    .Where(er => er.CurrencySnapshot.BaseCurrency == newRatesResponse.Base)
                    .Include(er => er.Currency)
                    .OrderByDescending(er => er.CurrencySnapshot.FetchTimestamp)
                    .GroupBy(er => er.Currency.Code)
                    .Select(g => g.First())
                    .ToDictionaryAsync(er => er.Currency.Code, er => er.Rate, cancellationToken);

                _logger.LogDebug("{Count} last exchange rate information for {Base} base was retrieved from the database.", newRatesResponse.Base, lastSnapshotRates.Count);

                var newDbSnapshot = new CurrencySnapshotModel
                {
                    FetchTimestamp = newRatesResponse.Date,
                    BaseCurrency = newRatesResponse.Base,
                    Rates = new List<ExchangeRateModel>()
                };

                bool hasChangesForThisSnapshot = false;

                foreach (var apiRatePair in newRatesResponse.Rates)
                {
                    string currencyCodeFromApi = apiRatePair.Key;
                    decimal rateFromApi = apiRatePair.Value;

                    decimal processedApiRate = Math.Round(rateFromApi, DefaultRatePrecision, MidpointRounding.AwayFromZero);

                    // DB'de bu para birimi için ID var mı kontrol et.
                    if (dbCurrenciesMap.TryGetValue(currencyCodeFromApi, out int currencyIdInDb))
                    {
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
                            var newDbExchangeRate = new ExchangeRateModel
                            {
                                CurrencyId = currencyIdInDb,
                                Rate = processedApiRate,
                            };
                            newDbSnapshot.Rates.Add(newDbExchangeRate);
                            hasChangesForThisSnapshot = true;

                            _logger.LogTrace("Change/new setup to be added to Snapshot: {Code} - API Rate: {ApiRate}, Processed Rate: {ProcessedRate}",
                                             currencyCodeFromApi, rateFromApi, processedApiRate);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No ID found in DB for currency code '{Code}'. This rate is being skipped.", currencyCodeFromApi);
                    }
                }

                bool isFirstSnapshotForBase = !await dbContext.CurrencySnapshots.AnyAsync(s => s.BaseCurrency == newRatesResponse.Base, cancellationToken);

                if (newDbSnapshot.Rates.Any() && (hasChangesForThisSnapshot || isFirstSnapshotForBase))
                {
                    dbContext.CurrencySnapshots.Add(newDbSnapshot);
                    int affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("A new snapshot with {RateCount} rate has been added to the database. Total affected rows (including relationships): {AffectedRows}",
                        newDbSnapshot.Rates.Count, affectedRows);
                }
                else
                {
                    _logger.LogInformation("No significant change in exchange rates has been detected or the snapshot is already up to date. No new snapshots have been added to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving a rate to the database.");
            }
        }

        private async Task UpdateSupportedCurrenciesAsync(MyDataContext dbContext, IHttpClientFactory httpClientFactory, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Desteklenen para birimleri güncelleniyor...");
            try
            {
                var client = httpClientFactory.CreateClient("CurrencyFreaksClient");

                if (string.IsNullOrWhiteSpace(_settings.SupportedCurrenciesUrl))
                {
                    _logger.LogError("SupportedCurrenciesUrl yapılandırılmamış.");
                    return;
                }

                var requestUri = _settings.SupportedCurrenciesUrl;

                var response = await client.GetAsync(requestUri, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync(stoppingToken);
                    var apiResponse = JsonSerializer.Deserialize<SupportedCurrenciesApiResponse>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.SupportedCurrenciesMap != null && apiResponse.SupportedCurrenciesMap.Any())
                    {
                        int newCurrenciesCount = 0;
                        int updatedCurrenciesCount = 0;

                        var existingDbCurrencies = await dbContext.Currencies.ToDictionaryAsync(c => c.Code, c => c, stoppingToken);

                        foreach (var apiKeyValuePair in apiResponse.SupportedCurrenciesMap)
                        {
                            string apiCurrencyCode = apiKeyValuePair.Key;
                            SupportedCurrencyDetail apiDetail = apiKeyValuePair.Value;

                            DateTime? availableFrom = null;
                            if (DateTime.TryParse(apiDetail.AvailableFromString, out DateTime fromDate))
                            {
                                availableFrom = fromDate;
                            }

                            DateTime? availableUntil = null;
                            if (DateTime.TryParse(apiDetail.AvailableUntilString, out DateTime untilDate))
                            {
                                availableUntil = untilDate;
                            }

                            if (existingDbCurrencies.TryGetValue(apiCurrencyCode, out CurrencyModel? dbCurrency))
                            {
                                // Var olanı güncelle
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
                                    _logger.LogDebug("Currency updated: {Code}", apiCurrencyCode);
                                }
                            }
                            else
                            {
                                // Yeni ekle
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
                                    LastUpdatedUtc = DateTime.UtcNow
                                };
                                await dbContext.Currencies.AddAsync(newCurrency, stoppingToken);
                                newCurrenciesCount++;
                                _logger.LogDebug("New currency added: {Code}", apiCurrencyCode);
                            }
                        }

                        if (newCurrenciesCount > 0 || updatedCurrenciesCount > 0)
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                        else
                        {
                            _logger.LogInformation("No changes were made to the database of supported currencies.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retrieve supported currency data from the API or came back empty.");
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
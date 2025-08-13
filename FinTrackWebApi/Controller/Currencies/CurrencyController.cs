using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.CurrencyDtos;
using FinTrackWebApi.Services.CurrencyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller.Currencies
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class CurrencyController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(
            ICurrencyService currencyService,
            ILogger<CurrencyController> logger,
            MyDataContext context
        )
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CurrencySummaryDto>>> GetAllCurrencies()
        {
            var currencies = await _context.Currencies
                .AsNoTracking()
                .Select(c => new CurrencySummaryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    IconUrl = c.IconUrl
                })
                .ToListAsync();

            if (currencies == null || !currencies.Any())
            {
                _logger.LogWarning("No currencies found in the database.");
                return NotFound("No currencies found.");
            }

            _logger.LogInformation("Retrieved {Count} currencies from the database.", currencies.Count);

            return Ok(currencies);
        }

        /// <summary>
        /// Belirtilen para biriminin detaylarını ve çapraz kur bilgilerini getirir.
        /// </summary>
        [HttpGet("{code}")]
        public async Task<ActionResult<SpecificCurrencyDto>> GetSpecificCurrency(string code)
        {
            code = code.ToUpper();

            var currencyModel = await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code);

            if (currencyModel == null)
            {
                _logger.LogWarning("Currency with code '{Code}' not found.", code);
                return NotFound($"Currency with code '{code}' not found.");
            }

            var latestUsdSnapshot = await _context.CurrencySnapshots
                .AsNoTracking()
                .Where(s => s.BaseCurrency == "USD")
                .OrderByDescending(s => s.FetchTimestamp)
                .Include(s => s.Rates)
                    .ThenInclude(r => r.Currency)
                .FirstOrDefaultAsync();

            var dto = new SpecificCurrencyDto
            {
                Id = currencyModel.Id,
                CurrencyCode = currencyModel.Code,
                CurrencyName = currencyModel.Name,
                CountryCode = currencyModel.CountryCode,
                CountryName = currencyModel.CountryName,
                Status = currencyModel.Status,
                AvailableFrom = currencyModel.AvailableFrom,
                AvailableUntil = currencyModel.AvailableUntil,
                IconUrl = currencyModel.IconUrl,
                RatesInfo = null
            };

            if (latestUsdSnapshot == null)
            {
                _logger.LogWarning("No USD-based currency snapshot found. Returning currency data without rates.");
                return Ok(dto);
            }

            var requestedCurrencyRateInUsd = latestUsdSnapshot.Rates
                .FirstOrDefault(r => r.Currency?.Code == code);

            if (requestedCurrencyRateInUsd == null || requestedCurrencyRateInUsd.Rate == 0)
            {
                _logger.LogWarning($"Rate for '{code}' not found in the latest USD snapshot. Returning currency data without rates.");
                return Ok(dto);
            }

            var baseRateForCalculation = requestedCurrencyRateInUsd.Rate;

            var calculatedRates = new CalculatedRatesDto
            {
                SnapshotTimestamp = latestUsdSnapshot.FetchTimestamp,
                BaseCurrency = code,
                CrossRates = new Dictionary<string, decimal>()
            };

            foreach (var rateFromSnapshot in latestUsdSnapshot.Rates)
            {
                if (rateFromSnapshot.Currency != null && !string.IsNullOrEmpty(rateFromSnapshot.Currency.Code))
                {
                    decimal crossRate = rateFromSnapshot.Rate / baseRateForCalculation;
                    calculatedRates.CrossRates[rateFromSnapshot.Currency.Code] = crossRate;
                }
            }

            dto.RatesInfo = calculatedRates;

            return Ok(dto);
        }

        private record SnapshotRate(DateTime FetchTimestamp, string CurrencyCode, decimal Rate);

        /// <summary>
        /// Belirtilen iki para birimi arasındaki geçmiş kur verilerini ve değişim analizini getirir.
        /// </summary>
        [HttpGet("{baseCode}/history/{targetCode}")]
        public async Task<ActionResult<CurrencyHistoryDto>> GetCurrencyHistory(
            string baseCode,
            string targetCode,
            [FromQuery] string period = "1M")
        {
            baseCode = baseCode.ToUpper();
            targetCode = targetCode.ToUpper();

            if (baseCode == targetCode) return BadRequest("Base and target currencies cannot be the same.");

            var endDate = DateTime.UtcNow;
            var startDate = period.ToUpper() switch
            {
                "1D" => endDate.AddDays(-1),
                "1W" => endDate.AddDays(-7),
                "3M" => endDate.AddMonths(-3),
                "1Y" => endDate.AddYears(-1),
                "YTD" => new DateTime(endDate.Year, 1, 1),
                _ => endDate.AddMonths(-1),
            };

            var allRatesFromHistory = await _context.ExchangeRates
                .AsNoTracking()
                .Where(r => r.CurrencySnapshot.BaseCurrency == "USD" && r.CurrencySnapshot.HasChanges)
                .Include(r => r.CurrencySnapshot)
                .Include(r => r.Currency)
                .OrderBy(r => r.CurrencySnapshot.FetchTimestamp)
                .Select(r => new { r.CurrencySnapshot.FetchTimestamp, r.Currency.Code, r.Rate })
                .ToListAsync();

            if (!allRatesFromHistory.Any())
            {
                return NotFound("No historical rate changes found in the database.");
            }

            var dailyRateChanges = allRatesFromHistory
                .GroupBy(r => r.FetchTimestamp.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(r => r.Code)
                          .ToDictionary(gr => gr.Key, gr => gr.Last().Rate)
                );

            var historicalRates = new List<HistoricalRatePointDto>();
            Dictionary<string, decimal> lastKnownRates = new Dictionary<string, decimal>();

            for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                var relevantDay = dailyRateChanges.Keys
                    .Where(d => d <= day)
                    .DefaultIfEmpty()
                    .Max();

                if (relevantDay != default && dailyRateChanges.TryGetValue(relevantDay, out var ratesForDay))
                {
                    lastKnownRates = ratesForDay;
                }

                if (!lastKnownRates.Any()) continue;

                decimal? rateForBase = baseCode == "USD" ? 1.0m : lastKnownRates.GetValueOrDefault(baseCode);
                decimal? rateForTarget = targetCode == "USD" ? 1.0m : lastKnownRates.GetValueOrDefault(targetCode);

                if (rateForBase.GetValueOrDefault() != 0 && rateForTarget.HasValue)
                {
                    var crossRate = rateForTarget.Value / rateForBase.Value;
                    historicalRates.Add(new HistoricalRatePointDto
                    {
                        Date = day,
                        Rate = crossRate
                    });
                }
            }

            if (!historicalRates.Any())
            {
                return NotFound($"Could not calculate cross-rates for {baseCode}/{targetCode} in the given period.");
            }

            decimal? dailyLow = null;
            decimal? dailyHigh = null;

            var dailyCalculationStartDate = endDate.AddDays(-1);

            var timestampsInLast24Hours = allRatesFromHistory
                .Where(r => r.FetchTimestamp >= dailyCalculationStartDate)
                .Select(r => r.FetchTimestamp)
                .Distinct()
                .OrderBy(t => t);

            var crossRatesLast24Hours = new List<decimal>();

            foreach (var timestamp in timestampsInLast24Hours)
            {
                var baseRateAtTime = baseCode == "USD" ? 1.0m : allRatesFromHistory
                    .LastOrDefault(r => r.Code == baseCode && r.FetchTimestamp <= timestamp)?.Rate;

                var targetRateAtTime = targetCode == "USD" ? 1.0m : allRatesFromHistory
                    .LastOrDefault(r => r.Code == targetCode && r.FetchTimestamp <= timestamp)?.Rate;

                if (baseRateAtTime.HasValue && baseRateAtTime.Value != 0 && targetRateAtTime.HasValue)
                {
                    crossRatesLast24Hours.Add(targetRateAtTime.Value / baseRateAtTime.Value);
                }
            }

            if (crossRatesLast24Hours.Any())
            {
                var finalRate = historicalRates.LastOrDefault()?.Rate;
                if (finalRate.HasValue) crossRatesLast24Hours.Add(finalRate.Value);

                dailyLow = crossRatesLast24Hours.Min();
                dailyHigh = crossRatesLast24Hours.Max();
            }

            var changeSummary = CalculateChangeSummary(historicalRates, dailyLow, dailyHigh);

            return Ok(new CurrencyHistoryDto
            {
                BaseCurrency = baseCode,
                TargetCurrency = targetCode,
                StartDate = historicalRates.First().Date,
                EndDate = historicalRates.Last().Date,
                HistoricalRates = historicalRates,
                ChangeSummary = changeSummary
            });
        }

        [HttpGet("convert")]
        public async Task<ActionResult<ConversionResultDto>> ConvertCurrency(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] decimal amount)
        {
            from = from.ToUpper();
            to = to.ToUpper();

            if (from == to)
            {
                return Ok(new ConversionResultDto
                {
                    From = from,
                    To = to,
                    Amount = amount,
                    Result = amount,
                    Rate = 1,
                    Timestamp = DateTime.UtcNow
                });
            }

            var latestUsdSnapshot = await _context.CurrencySnapshots
                .AsNoTracking()
                .Where(s => s.BaseCurrency == "USD")
                .OrderByDescending(s => s.FetchTimestamp)
                .Include(s => s.Rates)
                    .ThenInclude(r => r.Currency)
                .FirstOrDefaultAsync();

            if (latestUsdSnapshot == null || !latestUsdSnapshot.Rates.Any())
            {
                return NotFound("Exchange rate data is not available to perform conversion.");
            }

            var fromRateInUsd = from == "USD" ? 1.0m : latestUsdSnapshot.Rates
                .FirstOrDefault(r => r.Currency?.Code == from)?.Rate;

            var toRateInUsd = to == "USD" ? 1.0m : latestUsdSnapshot.Rates
                .FirstOrDefault(r => r.Currency?.Code == to)?.Rate;

            if (fromRateInUsd == null || fromRateInUsd.Value == 0)
            {
                return NotFound($"Source currency '{from}' not found or has an invalid rate.");
            }

            if (toRateInUsd == null)
            {
                return NotFound($"Target currency '{to}' not found.");
            }

            decimal crossRate = toRateInUsd.Value / fromRateInUsd.Value;
            decimal convertedAmount = amount * crossRate;

            var resultDto = new ConversionResultDto
            {
                From = from,
                To = to,
                Amount = amount,
                Result = convertedAmount,
                Rate = crossRate,
                Timestamp = latestUsdSnapshot.FetchTimestamp
            };

            return Ok(resultDto);
        }

        private ChangeSummaryDto CalculateChangeSummary(List<HistoricalRatePointDto> rates, decimal? dailyLow, decimal? dailyHigh)
        {
            var summary = new ChangeSummaryDto
            {
                DailyLow = dailyLow,
                DailyHigh = dailyHigh
            };

            if (rates == null || rates.Count < 2)
            {
                return summary;
            }

            var sortedRates = rates.OrderBy(r => r.Date).ToList();
            var latestRatePoint = sortedRates.Last();

            var dayBeforeLast = sortedRates[^2];
            if (dayBeforeLast.Rate != 0)
            {
                summary.DailyChangeValue = latestRatePoint.Rate - dayBeforeLast.Rate;
                summary.DailyChangePercentage = summary.DailyChangeValue / dayBeforeLast.Rate;
            }

            var date7DaysAgo = latestRatePoint.Date.AddDays(-7);
            var rate7DaysAgoPoint = sortedRates.LastOrDefault(r => r.Date <= date7DaysAgo);
            if (rate7DaysAgoPoint != null && rate7DaysAgoPoint.Rate != 0)
            {
                summary.WeeklyChangeValue = latestRatePoint.Rate - rate7DaysAgoPoint.Rate;
                summary.WeeklyChangePercentage = summary.WeeklyChangeValue / rate7DaysAgoPoint.Rate;
            }

            var date30DaysAgo = latestRatePoint.Date.AddMonths(-1);
            var rate30DaysAgoPoint = sortedRates.LastOrDefault(r => r.Date <= date30DaysAgo);
            if (rate30DaysAgoPoint != null && rate30DaysAgoPoint.Rate != 0)
            {
                summary.MonthlyChangeValue = latestRatePoint.Rate - rate30DaysAgoPoint.Rate;
                summary.MonthlyChangePercentage = summary.MonthlyChangeValue / rate30DaysAgoPoint.Rate;
            }

            return summary;
        }

        /// <summary>
        /// Belirtilen bir baz para birimine göre tüm diğer para birimlerinin en güncel kur bilgilerini listeler.
        /// </summary>
        /// <param name="baseCurrencyCode">Baz alınacak para biriminin 3 harfli kodu (örn: USD, TRY). Varsayılan: USD.</param>
        [HttpGet("latest/{baseCurrencyCode?}")]
        public async Task<ActionResult<LatestRatesResponseDto>> GetLatestRates(string baseCurrencyCode = "USD")
        {
            baseCurrencyCode = baseCurrencyCode.ToUpper();

            var latestUsdSnapshot = await _context.CurrencySnapshots
                .AsNoTracking()
                .Where(s => s.BaseCurrency == "USD")
                .OrderByDescending(s => s.FetchTimestamp)
                .Include(s => s.Rates)
                    .ThenInclude(r => r.Currency)
                .FirstOrDefaultAsync();

            if (latestUsdSnapshot == null || !latestUsdSnapshot.Rates.Any())
            {
                return NotFound("No exchange rate data found in the database.");
            }

            var baseRateInUsd = latestUsdSnapshot.Rates
                .FirstOrDefault(r => r.Currency?.Code == baseCurrencyCode)?.Rate;

            if (baseRateInUsd == null || baseRateInUsd.Value == 0)
            {
                if (baseCurrencyCode == "USD") baseRateInUsd = 1.0m;
                else return NotFound($"The specified base currency '{baseCurrencyCode}' is not found in the latest rate data.");
            }

            var rateDetails = new List<RateDetailDto>();
            foreach (var rate in latestUsdSnapshot.Rates)
            {
                if (rate.Currency?.Code == baseCurrencyCode) continue;

                if (rate.Currency != null)
                {
                    // Çapraz Kur Hesaplaması: (Hedefin USD Değeri) / (Bazın USD Değeri)
                    var calculatedRate = rate.Rate / baseRateInUsd.Value;

                    rateDetails.Add(new RateDetailDto
                    {
                        Id = rate.Currency.Id,
                        Code = rate.Currency.Code,
                        Name = rate.Currency.Name,
                        CountryName = rate.Currency.CountryName,
                        CountryCode = rate.Currency.CountryCode,
                        IconUrl = rate.Currency.IconUrl,
                        Rate = calculatedRate
                    });
                }
            }

            var responseDto = new LatestRatesResponseDto
            {
                BaseCurrency = baseCurrencyCode,
                SnapshotTimestamp = latestUsdSnapshot.FetchTimestamp,
                Rates = rateDetails.OrderBy(r => r.Code).ToList()
            };

            return Ok(responseDto);
        }


        [HttpGet("currency-by-code/{code}")]
        public async Task<IActionResult> GetCurrencyByCode(string code)
        {
            code = code.ToUpper();

            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);
            if (currency != null)
            {
                _logger.LogInformation("Currency with code {Code} found.", code);
                return Ok(currency);
            }

            _logger.LogWarning("Currency with code {Code} not found.", code);
            return NotFound($"Currency with code {code} not found.");
        }

        [HttpGet("currency-by-name/{name}")]
        public async Task<IActionResult> GetCurrencyByName(string name)
        {
            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);
            if (currency != null)
            {
                _logger.LogInformation("Currency with name {Name} found.", name);
                return Ok(currency);
            }

            _logger.LogWarning("Currency with name {Name} not found.", name);
            return NotFound($"Currency with name {name} not found.");
        }

        [HttpGet("currency-by-countryCode/{countryCode}")]
        public async Task<IActionResult> GetCurrencyByCountryCode(string countryCode)
        {
            countryCode = countryCode.ToUpper();

            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c =>
                c.CountryCode == countryCode
            );
            if (currency != null)
            {
                _logger.LogInformation("Currency with country code {CountryCode} found.", countryCode);
                return Ok(currency);
            }

            _logger.LogWarning("Currency with country code {CountryCode} not found.", countryCode);
            return NotFound($"Currency with country code {countryCode} not found.");
        }

        [HttpGet("currency-by-countryName/{countryName}")]
        public async Task<IActionResult> GetCurrencyByCountryName(string countryName)
        {
            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c =>
                c.CountryName.ToUpper() == countryName.ToUpper()
            );
            if (currency != null)
            {
                _logger.LogInformation("Currency with country name {CountryName} found.", countryName);
                return Ok(currency);
            }

            _logger.LogWarning("Currency with country name {CountryName} not found.", countryName);
            return NotFound($"Currency with country name {countryName} not found.");
        }
    }
}

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

        /// <summary>
        /// Belirtilen iki para birimi arasındaki geçmiş kur verilerini ve değişim analizini getirir.
        /// </summary>
        /// <param name="baseCode">Değeri izlenecek ana para birimi (örn: TRY).</param>
        /// <param name="targetCode">Karşılaştırma yapılacak hedef para birimi (örn: USD).</param>
        /// <param name="period">Veri aralığı ('1W', '1M', '3M', '1Y', 'YTD'). Varsayılan: '1M'.</param>
        /// <returns>Geçmiş kur verileri ve değişim özeti.</returns>
        [HttpGet("{baseCode}/history/{targetCode}")]
        public async Task<ActionResult<CurrencyHistoryDto>> GetCurrencyHistory(
            string baseCode,
            string targetCode,
            [FromQuery] string period = "1M")
        {
            baseCode = baseCode.ToUpper();
            targetCode = targetCode.ToUpper();

            var endDate = DateTime.UtcNow;
            var startDate = period.ToUpper() switch
            {
                "1W" => endDate.AddDays(-7),
                "3M" => endDate.AddMonths(-3),
                "1Y" => endDate.AddYears(-1),
                "YTD" => new DateTime(endDate.Year, 1, 1),
                _ => endDate.AddMonths(-1),
            };

            var snapshotsInPeriod = await _context.CurrencySnapshots
                .AsNoTracking()
                .Where(s => s.BaseCurrency == "USD" && s.FetchTimestamp >= startDate && s.FetchTimestamp <= endDate)
                .Include(s => s.Rates)
                    .ThenInclude(r => r.Currency)
                .OrderBy(s => s.FetchTimestamp)
                .ToListAsync();

            // Her gün için tek bir snapshot'a indirgiyorum. Ki performanslı olsun.
            var dailySnapshots = snapshotsInPeriod
                .GroupBy(s => s.FetchTimestamp.Date)
                .Select(g => g.OrderByDescending(s => s.FetchTimestamp).First())
                .ToList();

            if (!dailySnapshots.Any())
            {
                _logger.LogWarning("No historical data found for the period {StartDate} to {EndDate}.", startDate, endDate);
                return NotFound($"No historical data found for the period.");
            }

            // Çapraz Kur Hesaplaması ve Tarihsel Veri Noktalarını Oluşturma yapıyorum.
            var historicalRates = new List<HistoricalRatePointDto>();
            foreach (var snapshot in dailySnapshots)
            {
                var rateForBase = snapshot.Rates.FirstOrDefault(r => r.Currency?.Code == baseCode)?.Rate;
                var rateForTarget = snapshot.Rates.FirstOrDefault(r => r.Currency?.Code == targetCode)?.Rate;

                if (rateForBase.HasValue && rateForTarget.HasValue && rateForBase.Value != 0)
                {
                    // Çapraz Kur: 1 Base Doviz = X Target Doviz
                    // Örn: 1 TRY = (USD->USD rate) / (USD->TRY rate) = 1.0 / 40.0 = 0.025 USD
                    var crossRate = rateForTarget.Value / rateForBase.Value;
                    historicalRates.Add(new HistoricalRatePointDto
                    {
                        Date = snapshot.FetchTimestamp.Date,
                        Rate = crossRate
                    });
                }
            }

            if (!historicalRates.Any())
            {
                _logger.LogWarning("Could not calculate cross-rates for {BaseCode}/{TargetCode}.", baseCode, targetCode);
                return NotFound($"Could not calculate cross-rates for {baseCode}/{targetCode}.");
            }

            var changeSummary = CalculateChangeSummary(historicalRates);

            var resultDto = new CurrencyHistoryDto
            {
                BaseCurrency = baseCode,
                TargetCurrency = targetCode,
                StartDate = historicalRates.First().Date,
                EndDate = historicalRates.Last().Date,
                HistoricalRates = historicalRates,
                ChangeSummary = changeSummary
            };

            return Ok(resultDto);
        }

        private ChangeSummaryDto CalculateChangeSummary(List<HistoricalRatePointDto> rates)
        {
            if (rates.Count < 2) return new ChangeSummaryDto();

            var sortedRates = rates.OrderBy(r => r.Date).ToList();
            var latestRate = sortedRates.Last();

            var rate1DayAgo = sortedRates.FirstOrDefault(r => r.Date <= latestRate.Date.AddDays(-1));
            var rate7DaysAgo = sortedRates.FirstOrDefault(r => r.Date <= latestRate.Date.AddDays(-7));
            var rate30DaysAgo = sortedRates.FirstOrDefault(r => r.Date <= latestRate.Date.AddMonths(-1));

            var summary = new ChangeSummaryDto();

            summary.DailyChangeValue = rate1DayAgo != null ? latestRate.Rate - rate1DayAgo.Rate : (decimal?)null;
            summary.DailyChangePercentage = rate1DayAgo != null && rate1DayAgo.Rate != 0 ? (summary.DailyChangeValue / rate1DayAgo.Rate) * 100 : (decimal?)null;

            summary.WeeklyChangeValue = rate7DaysAgo != null ? latestRate.Rate - rate7DaysAgo.Rate : (decimal?)null;
            summary.WeeklyChangePercentage = rate7DaysAgo != null && rate7DaysAgo.Rate != 0 ? (summary.WeeklyChangeValue / rate7DaysAgo.Rate) * 100 : (decimal?)null;

            summary.MonthlyChangeValue = rate30DaysAgo != null ? latestRate.Rate - rate30DaysAgo.Rate : (decimal?)null;
            summary.MonthlyChangePercentage = rate30DaysAgo != null && rate30DaysAgo.Rate != 0 ? (summary.MonthlyChangeValue / rate30DaysAgo.Rate) * 100 : (decimal?)null;

            return summary;
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

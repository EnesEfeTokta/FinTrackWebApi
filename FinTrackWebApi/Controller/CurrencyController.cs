using FinTrackWebApi.Data;
using FinTrackWebApi.Services.CurrencyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class CurrencyController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger, MyDataContext context)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet("latest")]
        public IActionResult GetLatestRates()
        {
            var rates = _currencyService.GetLatestRatesFromCache();
            if (rates != null)
            {
                return Ok(rates);
            }
            return NotFound("No latest rates found.");
        }

        [HttpGet("rate/{targetCurrency}")]
        public IActionResult GetSpecificRate(string targetCurrency)
        {
            targetCurrency = targetCurrency.ToUpper();
            var rate = _currencyService.GetSpecificRateFromCache(targetCurrency);
            if (rate.HasValue)
            {
                return Ok(rate.Value);
            }
            return NotFound($"Rate for {targetCurrency} not found.");
        }

        [HttpGet("currencies")]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var currencies = await _context.Currencies.ToListAsync();
            if (currencies != null && currencies.Count > 0)
            {
                return Ok(currencies);
            }
            return NotFound("No latest rates found.");
        }

        [HttpGet("currency-by-code/{code}")]
        public async Task<IActionResult> GetCurrencyByCode(string code)
        {
            code = code.ToUpper();

            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == code);
            if (currency != null)
            {
                return Ok(currency);
            }
            return NotFound($"Currency with code {code} not found.");
        }

        [HttpGet("currency-by-name/{name}")]
        public async Task<IActionResult> GetCurrencyByName(string name)
        {
            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Name == name);
            if (currency != null)
            {
                return Ok(currency);
            }
            return NotFound($"Currency with name {name} not found.");
        }

        [HttpGet("currency-by-countryCode/{countryCode}")]
        public async Task<IActionResult> GetCurrencyByCountryCode(string countryCode)
        {
            countryCode = countryCode.ToUpper();

            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.CountryCode == countryCode);
            if (currency != null)
            {
                return Ok(currency);
            }
            return NotFound($"Currency with country code {countryCode} not found.");
        }

        [HttpGet("currency-by-countryName/{countryName}")]
        public async Task<IActionResult> GetCurrencyByCountryName(string countryName)
        {
            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.CountryCode == countryName);
            if (currency != null)
            {
                return Ok(currency);
            }
            return NotFound($"Currency with country name {countryName} not found.");
        }
    }
}

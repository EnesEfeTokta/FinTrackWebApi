using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Services.CurrencyServices;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
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
            var rate = _currencyService.GetSpecificRateFromCache(targetCurrency);
            if (rate.HasValue)
            {
                return Ok(rate.Value);
            }
            return NotFound($"Rate for {targetCurrency} not found.");
        }
    }
}

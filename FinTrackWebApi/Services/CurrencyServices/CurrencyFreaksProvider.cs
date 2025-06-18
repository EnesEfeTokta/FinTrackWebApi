using System.Text.Json;
using System.Text.Json.Serialization;
using FinTrackWebApi.Services.CurrencyServices.Models;
using Microsoft.Extensions.Options;

namespace FinTrackWebApi.Services.CurrencyServices
{
    public class CurrencyFreaksProvider : ICurrencyDataProvider
    {
        private readonly ILogger<CurrencyFreaksProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyFreaksSettings _settings;

        public CurrencyFreaksProvider(
            ILogger<CurrencyFreaksProvider> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<CurrencyFreaksSettings> settings
        )
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public async Task<CurrencyFreaksResponse?> GetLatestRatesAsync(
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation("Fetching latest currency rates from CurrencyFreaks API.");

            try
            {
                var client = _httpClientFactory.CreateClient("CurrencyFreaksClient");
                var requestUri = $"rates/latest?apikey={_settings.ApiKey}";

                var request = await client.GetAsync(requestUri, cancellationToken);

                if (request.IsSuccessStatusCode)
                {
                    var responseStream = await request.Content.ReadAsStreamAsync(cancellationToken);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling =
                            JsonNumberHandling.AllowReadingFromString
                            | JsonNumberHandling.WriteAsString,
                    };

                    var ratesResponse =
                        await JsonSerializer.DeserializeAsync<CurrencyFreaksResponse>(
                            responseStream,
                            options,
                            cancellationToken
                        );

                    if (
                        ratesResponse != null
                        && ratesResponse.Rates != null
                        && ratesResponse.Rates.Any()
                    )
                    {
                        _logger.LogInformation("Successfully fetched latest currency rates.");
                        return ratesResponse;
                    }
                    else
                    {
                        _logger.LogError(
                            "Failed to deserialize the response from CurrencyFreaks API."
                        );
                        return null;
                    }
                }
                else
                {
                    _logger.LogError(
                        $"Failed to fetch latest currency rates. Status code: {request.StatusCode}"
                    );
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching latest currency rates.");
                return null;
            }
        }
    }
}

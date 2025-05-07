using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Stripe;

namespace FinTrackWebApi.Services.PaymentService
{
    [Authorize]
    public class StripePaymentService : IPaymentService
    {
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IOptions<StripeSettings> stripeSettings, ILogger<StripePaymentService> logger)
        {
            _stripeSettings = stripeSettings;
            _logger = logger;
            StripeConfiguration.ApiKey = _stripeSettings.Value.SecretKey;
        }

        public async Task<PaymentIntent?> CreatePaymentIntentAsync(decimal amount, string currency, int userId, string description, Dictionary<string, string>? metadata = null)
        {
            try
            {
                long amountInCents = Convert.ToInt32(amount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency.ToLower(),
                    Description = description,

                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },

                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", userId.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation("Created PaymentIntent {PaymentIntentId} for user {UserId} with amount {Amount} {Currency}",
                    paymentIntent.Id, userId, amount, currency);

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while creating PaymentIntent for user {UserId}: {Message}", userId, ex.Message);
                throw new Exception("Payment processing error. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating PaymentIntent for user {UserId}: {Message}", userId, ex.Message);
                throw new Exception("An unexpected error occurred. Please try again later.");
            }
        }
    }
}

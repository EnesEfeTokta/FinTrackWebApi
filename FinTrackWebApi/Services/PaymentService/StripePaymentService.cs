using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace FinTrackWebApi.Services.PaymentService
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            IOptions<StripeSettings> stripeSettingsOptions,
            ILogger<StripePaymentService> logger
        )
        {
            _stripeSettings = stripeSettingsOptions.Value;
            _logger = logger;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<Session?> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string planName,
            string? planDescription,
            string successUrl,
            string cancelUrl,
            string? clientReferenceId = null,
            Dictionary<string, string>? metadata = null
        )
        {
            try
            {
                // Tutar kuruş/cent cinsinden olmalı
                long amountInSmallestUnit = Convert.ToInt64(amount * 100);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmountDecimal = amountInSmallestUnit, // decimal olarak da verilebilir
                                Currency = currency.ToLower(),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = planName,
                                    Description = planDescription ?? string.Empty,
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment", // Tek seferlik ödeme için
                    SuccessUrl = successUrl, // Örn: "https://yourdomain.com/payment-success?session_id={CHECKOUT_SESSION_ID}"
                    CancelUrl = cancelUrl, // Örn: "https://yourdomain.com/payment-cancelled"
                    ClientReferenceId = clientReferenceId, // Örn: UserId veya OrderId
                    Metadata = metadata ?? new Dictionary<string, string>(), // Metadata boşsa yeni dictionary ata
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Created Stripe Checkout Session {SessionId} for client {ClientRefId} with amount {Amount} {Currency}",
                    session.Id,
                    clientReferenceId ?? "N/A",
                    amount,
                    currency
                );

                return session;
            }
            catch (StripeException ex)
            {
                _logger.LogError(
                    ex,
                    "Stripe error while creating Checkout Session for client {ClientRefId}: {Message}",
                    clientReferenceId ?? "N/A",
                    ex.StripeError?.Message ?? ex.Message
                );
                // Burada Exception fırlatmak yerine null dönmek veya özel bir sonuç objesi dönmek daha iyi olabilir.
                // Controller'ın hatayı uygun şekilde işlemesi için.
                throw; // Veya throw new ApplicationException($"Payment processing error: {ex.StripeError?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Generic error while creating Checkout Session for client {ClientRefId}: {Message}",
                    clientReferenceId ?? "N/A",
                    ex.Message
                );
                throw; // Veya throw new ApplicationException("An unexpected error occurred during payment processing.", ex);
            }
        }
    }
}

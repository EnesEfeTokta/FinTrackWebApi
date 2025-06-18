using Stripe.Checkout;

namespace FinTrackWebApi.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<Session?> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string planName,
            string? planDescription,
            string successUrl,
            string cancelUrl,
            string? clientReferenceId = null,
            Dictionary<string, string>? metadata = null
        );
    }
}

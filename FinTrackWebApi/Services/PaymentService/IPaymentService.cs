using Stripe;

namespace FinTrackWebApi.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<PaymentIntent?> CreatePaymentIntentAsync(decimal amount, string currency, int userId, string description, Dictionary<string, string>? metadata = null);
    }
}

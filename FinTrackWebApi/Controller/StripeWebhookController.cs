using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.PaymentService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FinTrackWebApi.Controller
{
    [Route("api/stripe/webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeSettings _stripeSettings;
        private readonly MyDataContext _context;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IOptions<StripeSettings> stripeSettingsOptions,
            MyDataContext context,
            ILogger<StripeWebhookController> logger)
        {
            _stripeSettings = stripeSettingsOptions.Value;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string endpointSecret = _stripeSettings.WebhookSecret;

            if (string.IsNullOrEmpty(endpointSecret))
            {
                _logger.LogError("Stripe Webhook Secret is not configured.");
                return BadRequest("Webhook secret not configured.");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );

                _logger.LogInformation("Stripe Webhook Received: Event Type = {EventType}, Event ID = {EventId}", stripeEvent.Type, stripeEvent.Id);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session == null)
                    {
                        _logger.LogError("Failed to cast Stripe event data to Session object for Event ID {EventId}.", stripeEvent.Id);
                        return BadRequest("Invalid session data.");
                    }

                    if (session.PaymentStatus == "paid")
                    {
                        _logger.LogInformation("CheckoutSessionCompleted: Session ID = {SessionId}, PaymentStatus = {PaymentStatus}", session.Id, session.PaymentStatus);

                        if (session.Metadata == null)
                        {
                            _logger.LogError("Metadata is missing in Stripe Session {SessionId}.", session.Id);
                            return BadRequest("Metadata is missing.");
                        }

                        session.Metadata.TryGetValue("UserMembershipId", out var userMembershipIdStr);
                        session.Metadata.TryGetValue("PaymentId", out var paymentIdStr);

                        if (int.TryParse(userMembershipIdStr, out int userMembershipId) &&
                            int.TryParse(paymentIdStr, out int paymentId))
                        {
                            _logger.LogInformation("Processing UserMembershipId: {UserMembershipId}, PaymentId: {PaymentId} from metadata.", userMembershipId, paymentId);

                            var payment = await _context.Payments.FindAsync(paymentId);
                            if (payment != null && payment.Status == PaymentStatus.Succeeded)
                            {
                                _logger.LogInformation("PaymentId {PaymentId} has already been processed and marked as Succeeded. Skipping update.", paymentId);
                                return Ok();
                            }

                            var userMembership = await _context.UserMemberships
                                                        .Include(um => um.Plan)
                                                        .FirstOrDefaultAsync(um => um.UserMembershipId == userMembershipId);

                            if (userMembership == null)
                            {
                                _logger.LogError("UserMembership (ID: {UserMembershipId}) not found in database for Stripe Session {SessionId}.", userMembershipId, session.Id);
                                return NotFound($"UserMembership with ID {userMembershipId} not found.");
                            }

                            if (payment == null)
                            {
                                _logger.LogError("Payment (ID: {PaymentId}) not found in database for Stripe Session {SessionId}.", paymentId, session.Id);
                                return NotFound($"Payment with ID {paymentId} not found.");
                            }

                            if (userMembership.Status == MembershipStatus.PendingPayment && payment.Status == PaymentStatus.Pending)
                            {
                                userMembership.Status = MembershipStatus.Active;
                                userMembership.StartDate = DateTime.UtcNow;

                                if (userMembership.Plan != null && userMembership.Plan.DurationInDays.HasValue)
                                {
                                    userMembership.EndDate = userMembership.StartDate.AddDays(userMembership.Plan.DurationInDays.Value);
                                }
                                else if (userMembership.Plan != null)
                                {
                                    int defaultDuration = (userMembership.Plan.Price == 0) ? 365 * 100 : 30;
                                    userMembership.EndDate = userMembership.StartDate.AddDays(defaultDuration);
                                }

                                payment.Status = PaymentStatus.Succeeded;
                                payment.TransactionId = session.PaymentIntentId ?? session.Id;
                                payment.GatewayResponse = $"Stripe Checkout Session ID: {session.Id}";
                                payment.PaymentDate = DateTime.UtcNow;

                                await _context.SaveChangesAsync();
                                _logger.LogInformation("UserMembership {UserMembershipId} activated and Payment {PaymentId} marked as Succeeded for Stripe Session {SessionId}.", userMembershipId, paymentId, session.Id);
                            }
                            else
                            {
                                _logger.LogWarning("UserMembership {UserMembershipId} (Status: {UserMembershipStatus}) or Payment {PaymentId} (Status: {PaymentStatus}) was not in a pending state. Skipping update.",
                                    userMembershipId, userMembership.Status, paymentId, payment.Status);
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to parse UserMembershipId ('{UserMembershipIdStr}') or PaymentId ('{PaymentIdStr}') from metadata for Stripe Session {SessionId}.",
                                userMembershipIdStr, paymentIdStr, session.Id);
                            return BadRequest("Invalid metadata format.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("CheckoutSessionCompleted for Session ID = {SessionId}, but PaymentStatus is '{PaymentStatus}'. No action taken.", session.Id, session.PaymentStatus);
                    }
                }
                // TODO: Diğer önemli Stripe olaylarını burada yapılacak.
                // Örneğin: Events.PaymentIntentSucceeded, Events.PaymentIntentPaymentFailed
                else
                {
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe Webhook Error: Invalid Signature or other Stripe issue. Message: {StripeErrorMessage}", e.StripeError?.Message ?? e.Message);
                return BadRequest(new { error = "Webhook signature verification failed or Stripe error." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Generic Webhook Error processing Stripe event.");
                return StatusCode(500, new { error = "Internal server error while processing webhook." });
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.PaymentService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FinTrackWebApi.Services.EmailService;
using System.Globalization;

namespace FinTrackWebApi.Controller
{
    [Route("api/stripe/webhook")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeSettings _stripeSettings;
        private readonly MyDataContext _context;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IEmailSender _emailSender;

        public StripeWebhookController(
            IOptions<StripeSettings> stripeSettingsOptions,
            MyDataContext context,
            ILogger<StripeWebhookController> logger,
            IEmailSender emailSender)
        {
            _stripeSettings = stripeSettingsOptions.Value;
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
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
                                                        .Include(um => um.User)
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

                                // E-posta Gönderme İşlemi
                                if (userMembership.User != null && !string.IsNullOrEmpty(userMembership.User.Email))
                                {
                                    try
                                    {
                                        string emailBody = string.Empty;
                                        // TODO: Path 'i dinamik olarak ayarlanacak.
                                        using (StreamReader reader = new StreamReader(@"C:\Users\alfac\OneDrive\Belgeler\GitHub\FinTrackWebApi\FinTrackWebApi\Services\EmailService\EmailHtmlSchemes\MembershipPaymentBillingScheme.html"))
                                        {
                                            emailBody = await reader.ReadToEndAsync();
                                        }

                                        emailBody = emailBody.Replace("[LOGO_URL]", "null"); // TODO: Gerçek logo URL'niz.
                                        emailBody = emailBody.Replace("[Username]", userMembership.User.UserName ?? "Valued User");
                                        emailBody = emailBody.Replace("[MembershipPlanName]", userMembership.Plan?.Name ?? "N/A");
                                        emailBody = emailBody.Replace("[DASHBOARD_LINK]", "https://yourdomain.com/dashboard"); // Gerçek dashboard linkiniz

                                        string invoiceNumber = $"INV-{payment.PaymentId}-{DateTime.UtcNow:yyyyMMdd}"; // Basit bir fatura no
                                        emailBody = emailBody.Replace("[InvoiceNumber]", invoiceNumber);
                                        emailBody = emailBody.Replace("[TransactionDate]", payment.PaymentDate.ToString("dd MMMM yyyy HH:mm", CultureInfo.InvariantCulture));
                                        emailBody = emailBody.Replace("[StripeTransactionId]", payment.TransactionId ?? "N/A");
                                        emailBody = emailBody.Replace("[MembershipStartDate]", userMembership.StartDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture));
                                        emailBody = emailBody.Replace("[MembershipEndDate]", userMembership.EndDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture));

                                        string paymentMethodDisplayForEmail = "Card Payment"; // Varsayılan

                                        if (!string.IsNullOrEmpty(session.PaymentIntentId))
                                        {
                                            try
                                            {
                                                var paymentIntentService = new PaymentIntentService();
                                                PaymentIntent paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId, new PaymentIntentGetOptions
                                                {
                                                    Expand = new List<string> { "payment_method" } // Ödeme yöntemi detaylarını da çek
                                                });

                                                if (paymentIntent?.PaymentMethod?.Card != null)
                                                {
                                                    paymentMethodDisplayForEmail = $"{paymentIntent.PaymentMethod.Card.Brand?.ToUpper()} ending in {paymentIntent.PaymentMethod.Card.Last4}";
                                                }
                                                else if (paymentIntent?.PaymentMethod?.Type != null)
                                                {
                                                    paymentMethodDisplayForEmail = $"Paid using {paymentIntent.PaymentMethod.Type}";
                                                }
                                                _logger.LogInformation("Fetched PaymentIntent {PaymentIntentId} for email details. PaymentMethod Type: {PaymentMethodType}", paymentIntent.Id, paymentIntent.PaymentMethod?.Type);
                                            }
                                            catch (StripeException ex)
                                            {
                                                _logger.LogError(ex, "StripeException while fetching PaymentIntent {PaymentIntentId} for email details.", session.PaymentIntentId);
                                                // paymentMethodDisplayForEmail varsayılan değerde kalır
                                            }
                                        }

                                        emailBody = emailBody.Replace("[PaymentMethodDetails]", paymentMethodDisplayForEmail);

                                        emailBody = emailBody.Replace("[TotalAmount]", payment.Amount.ToString("F2", CultureInfo.InvariantCulture));
                                        emailBody = emailBody.Replace("[Currency]", payment.Currency.ToUpper());

                                        emailBody = emailBody.Replace("[SUPPORT_EMAIL]", "enesefetokta009@gmail.com");
                                        emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.Year.ToString());
                                        emailBody = emailBody.Replace("[UNSUBSCRIBE_LINK]", "https://yourdomain.com/unsubscribe");
                                        emailBody = emailBody.Replace("[TERMS_LINK]", "https://yourdomain.com/terms");


                                        string emailSubject = "Your FinTrack Payment Confirmation & Invoice";

                                        await _emailSender.SendEmailAsync(userMembership.User.Email, emailSubject, emailBody);
                                        _logger.LogInformation("Successfully sent payment confirmation email to {UserEmail} for UserMembershipId {UserMembershipId}.", userMembership.User.Email, userMembershipId);
                                    }
                                    catch (Exception emailEx)
                                    {
                                        _logger.LogError(emailEx, "Failed to send payment confirmation email for UserMembershipId {UserMembershipId}.", userMembershipId);
                                        // E-posta gönderilemese bile ana işlem (üyelik aktivasyonu) başarılı oldu.
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("User email not found for UserMembershipId {UserMembershipId}, cannot send confirmation email.", userMembershipId);
                                }

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
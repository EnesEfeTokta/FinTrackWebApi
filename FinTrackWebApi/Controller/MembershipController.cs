using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Services.PaymentService;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MembershipController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<MembershipController> _logger;
        private readonly IPaymentService _paymentService;

        public MembershipController(MyDataContext context, ILogger<MembershipController> logger, IPaymentService paymentService)
        {
            _context = context;
            _logger = logger;
            _paymentService = paymentService;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid for user {UserName}.", User.Identity?.Name ?? "Unknown");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserMembershipDto>> GetCurrentUserActiveMembership()
        {
            var userId = GetAuthenticatedUserId();
            var activeMembership = await _context.UserMemberships
                .Include(um => um.Plan)
                .Where(um => um.UserId == userId && um.Status == MembershipStatus.Active && um.EndDate > DateTime.UtcNow)
                .OrderByDescending(um => um.EndDate)
                .Select(um => new UserMembershipDto
                {
                    UserMembershipId = um.UserMembershipId,
                    PlanId = um.MembershipPlanId,
                    PlanName = um.Plan.Name,
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew
                })
                .FirstOrDefaultAsync();
            return activeMembership == null ? NotFound("No active membership found.") : Ok(activeMembership);
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<UserMembershipDto>>> GetUserMembershipHistory()
        {
            var userId = GetAuthenticatedUserId();
            var memberships = await _context.UserMemberships
                .Include(um => um.Plan)
                .Where(um => um.UserId == userId)
                .OrderByDescending(um => um.StartDate)
                .Select(um => new UserMembershipDto
                {
                    UserMembershipId = um.UserMembershipId,
                    PlanId = um.MembershipPlanId,
                    PlanName = um.Plan.Name,
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew
                })
                .ToListAsync();
            return Ok(memberships);
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] SubscriptionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetAuthenticatedUserId();
            var planToSubscribe = await _context.MembershipPlans.FindAsync(request.PlanId);

            if (planToSubscribe == null || !planToSubscribe.IsActive)
            {
                _logger.LogWarning("PlanId {RequestedPlanId} not found or not active for UserId {UserId}.", request.PlanId, userId);
                return NotFound(new { message = "Selected plan is not valid or active." });
            }

            var existingActiveMembership = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserId == userId && um.MembershipPlanId == request.PlanId && um.Status == MembershipStatus.Active && um.EndDate > DateTime.UtcNow);

            if (existingActiveMembership != null)
            {
                _logger.LogInformation("User {UserId} already has an active subscription to PlanId {PlanId}.", userId, request.PlanId);
                return BadRequest(new { message = "You already have an active subscription to this plan." });
            }

            if (planToSubscribe.Price == 0)
            {
                var freeMembership = new UserMembershipModel
                {
                    UserId = userId,
                    MembershipPlanId = planToSubscribe.MembershipPlanId,
                    StartDate = DateTime.UtcNow,
                    EndDate = planToSubscribe.DurationInDays.HasValue ? DateTime.UtcNow.AddDays(planToSubscribe.DurationInDays.Value) : DateTime.UtcNow.AddYears(100),
                    Status = MembershipStatus.Active,
                    AutoRenew = false
                };
                _context.UserMemberships.Add(freeMembership);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully subscribed User {UserId} to free PlanId {PlanId}. Membership ID: {MembershipId}", userId, freeMembership.MembershipPlanId, freeMembership.UserMembershipId);
                return Ok(new { message = "Successfully subscribed to the free plan.", userMembershipId = freeMembership.UserMembershipId, sessionId = (string?)null, checkoutUrl = (string?)null });
            }

            var newMembership = new UserMembershipModel
            {
                UserId = userId,
                MembershipPlanId = planToSubscribe.MembershipPlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(planToSubscribe.DurationInDays ?? 30),
                Status = MembershipStatus.PendingPayment,
                AutoRenew = request.AutoRenew
            };
            _context.UserMemberships.Add(newMembership);
            await _context.SaveChangesAsync();

            var newPayment = new PaymentModel
            {
                UserId = userId,
                UserMembershipId = newMembership.UserMembershipId,
                Amount = planToSubscribe.Price,
                Currency = planToSubscribe.Currency.ToUpper(),
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };
            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            var domain = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{domain}/payment-success?session_id={{CHECKOUT_SESSION_ID}}&membership_id={newMembership.UserMembershipId}";
            var cancelUrl = $"{domain}/payment-cancelled";

            var metadata = new Dictionary<string, string>
            {
                { "UserMembershipId", newMembership.UserMembershipId.ToString() },
                { "PaymentId", newPayment.PaymentId.ToString() },
                { "PlanId", planToSubscribe.MembershipPlanId.ToString() },
                { "UserId", userId.ToString() }
            };

            try
            {
                var session = await _paymentService.CreateCheckoutSessionAsync(
                    planToSubscribe.Price,
                    planToSubscribe.Currency,
                    planToSubscribe.Name,
                    planToSubscribe.Description,
                    successUrl,
                    cancelUrl,
                    userId.ToString(),
                    metadata
                );

                if (session == null)
                {
                    _logger.LogError("PaymentService returned null for Checkout Session. UserId: {UserId}, PlanId: {PlanId}.", userId, planToSubscribe.MembershipPlanId);
                    await MarkSubscriptionAsFailed(newMembership.UserMembershipId, newPayment.PaymentId, "Failed to initiate Stripe session via PaymentService.");
                    return StatusCode(500, new { message = "Could not initiate payment session." });
                }

                _logger.LogInformation("Stripe Checkout Session {SessionId} created for UserMembershipId {UserMembershipId}. User will be redirected.", session.Id, newMembership.UserMembershipId);
                return Ok(new { sessionId = session.Id, checkoutUrl = session.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception creating Stripe Checkout Session. UserId: {UserId}, PlanId: {PlanId}.", userId, planToSubscribe.MembershipPlanId);
                await MarkSubscriptionAsFailed(newMembership.UserMembershipId, newPayment.PaymentId, $"Error during Stripe session creation: {ex.Message}");
                return BadRequest(new { message = "An error occurred while initiating the payment: " + ex.Message });
            }
        }

        private async Task MarkSubscriptionAsFailed(int membershipId, int paymentId, string gatewayResponseMessage)
        {
            var membership = await _context.UserMemberships.FindAsync(membershipId);
            var payment = await _context.Payments.FindAsync(paymentId);

            if (membership != null && membership.Status == MembershipStatus.PendingPayment)
            {
                membership.Status = MembershipStatus.FailedPayment;
            }
            if (payment != null && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Failed;
                payment.GatewayResponse = gatewayResponseMessage;
            }
            if ((membership != null && _context.Entry(membership).State == EntityState.Modified) ||
                (payment != null && _context.Entry(payment).State == EntityState.Modified))
            {
                await _context.SaveChangesAsync();
            }
        }

        [HttpPost("{userMembershipId}/cancel")]
        public async Task<IActionResult> CancelSubscription(int userMembershipId)
        {
            var userId = GetAuthenticatedUserId();
            var membershipToCancel = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserMembershipId == userMembershipId && um.UserId == userId);

            if (membershipToCancel == null)
            {
                return NotFound("Membership not found or you don't have permission to cancel it.");
            }

            if (membershipToCancel.Status != MembershipStatus.Active)
            {
                return BadRequest("Only active memberships can be cancelled.");
            }

            membershipToCancel.AutoRenew = false;
            membershipToCancel.Status = MembershipStatus.Cancelled;
            membershipToCancel.CancellationDate = DateTime.UtcNow;

            _context.Entry(membershipToCancel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subscription cancellation requested. It will expire on " + membershipToCancel.EndDate.ToShortDateString() });
        }
    }
}
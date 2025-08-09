using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.MembershipPlansDtos;
using FinTrackWebApi.Dtos.UserMembershipDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Membership;
using FinTrackWebApi.Models.User;
using FinTrackWebApi.Services.PaymentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Memberships
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    [Authorize] // TODO: [TEST]
    public class MembershipController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<MembershipController> _logger;
        private readonly IPaymentService _paymentService;

        public MembershipController(
            MyDataContext context,
            ILogger<MembershipController> logger,
            IPaymentService paymentService
        )
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
                _logger.LogError(
                    "Authenticated user ID claim (NameIdentifier) not found or invalid for user {UserName}.",
                    User.Identity?.Name ?? "Unknown"
                );
                throw new UnauthorizedAccessException(
                    "User ID cannot be determined from the token."
                );
            }
            return userId;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUserActiveMembership()
        {
            var userId = GetAuthenticatedUserId();
            var activeMembership = await _context
                .UserMemberships.Include(um => um.Plan)
                .Where(um =>
                    um.UserId == userId
                    && um.Status == MembershipStatusType.Active
                    && um.EndDate > DateTime.UtcNow
                )
                .OrderByDescending(um => um.EndDate)
                .Select(um => new UserMembershipDto
                {
                    Id = um.Id,
                    PlanId = um.MembershipPlanId,
                    PlanName = um.Plan.Name,
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew,
                })
                .FirstOrDefaultAsync();
            return activeMembership == null
                ? NotFound("No active membership found.")
                : Ok(activeMembership);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetUserMembershipHistory()
        {
            var userId = GetAuthenticatedUserId();

            var memberships = await _context
                .UserMemberships.Include(um => um.Plan)
                .Where(um => um.UserId == userId)
                .OrderByDescending(um => um.StartDate)
                .AsNoTracking()
                .Select(um => new UserMembershipDto
                {
                    Id = um.Id,
                    PlanId = um.MembershipPlanId,
                    PlanName = um.Plan.Name,
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew,
                })
                .ToListAsync();
            return Ok(memberships);
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailablePlans()
        {
            try
            {
                var plans = await _context.MembershipPlans
                    .Where(p => p.IsActive).AsNoTracking()
                    .Select(p => new PlanFeatureDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Currency = p.Currency ?? BaseCurrencyType.Error,
                        BillingCycle = p.BillingCycle ?? BillingCycleType.Monthly,
                        DurationInDays = p.DurationInDays,
                        Reporting = p.Reporting,
                        Emailing = p.Emailing,
                        Budgeting = p.Budgeting,
                        Accounts = p.Accounts,
                        PrioritySupport = p.PrioritySupport,
                    })
                    .ToListAsync();
                if (plans == null || !plans.Any())
                {
                    _logger.LogInformation("No active membership plans found.");
                    return NotFound(new { message = "No active membership plans available." });
                }

                _logger.LogInformation("Retrieved {PlanCount} active membership plans.", plans.Count);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available membership plans.");
                return StatusCode(500, new { message = "An error occurred while retrieving plans." });
            }
        }

        [HttpGet("plan/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailablePlans(int Id)
        {
            try
            {
                var plan = await _context.MembershipPlans
                    .Where(p => p.IsActive && p.Id == Id)
                    .AsNoTracking()
                    .Select(p => new PlanFeatureDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Currency = p.Currency ?? BaseCurrencyType.Error,
                        BillingCycle = p.BillingCycle ?? BillingCycleType.Monthly,
                        DurationInDays = p.DurationInDays,
                        Reporting = p.Reporting,
                        Emailing = p.Emailing,
                        Budgeting = p.Budgeting,
                        Accounts = p.Accounts,
                        PrioritySupport = p.PrioritySupport,
                    })
                    .FirstOrDefaultAsync();
                if (plan == null)
                {
                    _logger.LogInformation("No active membership plan found with Id {PlanId}.", Id);
                    return NotFound(new { message = "No active membership plan available." });
                }

                _logger.LogInformation(
                    "Retrieved active membership plan with Id {PlanId}.",
                    plan.Id
                );
                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available membership plans.");
                return StatusCode(500, new { message = "An error occurred while retrieving plans." });
            }
        }

        [HttpPost("plan")]
        public async Task<IActionResult> CreateMembershipPlan(
            [FromBody] PlanFeatureCreateDto planDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Invalid model state for creating membership plan: {Errors}",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    );
                    return BadRequest(ModelState);
                }

                var newPlan = new MembershipPlanModel
                {
                    Name = planDto.PlanName,
                    Description = planDto.PlanDescription,
                    Price = planDto.Price,
                    Currency = planDto.Currency,
                    BillingCycle = planDto.BillingCycle,
                    DurationInDays = planDto.DurationInDays,
                    Reporting = planDto.Reporting,
                    Emailing = planDto.Emailing,
                    Budgeting = planDto.Budgeting,
                    Accounts = planDto.Accounts,
                    PrioritySupport = planDto.PrioritySupport,
                    IsActive = planDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.MembershipPlans.Add(newPlan);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new membership plan with Id {PlanId} for user {UserId}.",
                    newPlan.Id,
                    GetAuthenticatedUserId()
                );
                return CreatedAtAction(nameof(GetAvailablePlans), new { Id = newPlan.Id }, newPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new membership plan.");
                return StatusCode(500, new { message = "An error occurred while creating the plan." });
            }
        }

        [HttpPut("plan/{Id}")]
        public async Task<IActionResult> UpdateMembershipPlan(
            int Id,
            [FromBody] PlanFeatureUpdateDto planDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingPlan = await _context.MembershipPlans.FindAsync(Id);
                if (existingPlan == null)
                {
                    _logger.LogWarning(
                        "Attempted to update non-existent membership plan with Id {PlanId}.",
                        Id
                    );
                    return NotFound(new { message = "Membership plan not found." });
                }

                existingPlan.Name = planDto.PlanName;
                existingPlan.Description = planDto.PlanDescription;
                existingPlan.Price = planDto.Price;
                existingPlan.Currency = planDto.Currency;
                existingPlan.BillingCycle = planDto.BillingCycle;
                existingPlan.DurationInDays = planDto.DurationInDays;
                existingPlan.Reporting = planDto.Reporting;
                existingPlan.Emailing = planDto.Emailing;
                existingPlan.Budgeting = planDto.Budgeting;
                existingPlan.Accounts = planDto.Accounts;
                existingPlan.PrioritySupport = planDto.PrioritySupport;
                existingPlan.IsActive = planDto.IsActive;
                existingPlan.UpdatedAt = DateTime.UtcNow;

                _context.Entry(existingPlan).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated membership plan with Id {PlanId} for user {UserId}.",
                    Id,
                    GetAuthenticatedUserId()
                );
                return Ok(existingPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership plan with Id {PlanId}.", Id);
                return StatusCode(500, new { message = "An error occurred while updating the plan." });
            }
        }

        [HttpDelete("plan/{Id}")]
        public async Task<IActionResult> DeleteMembershipPlan(int Id)
        {
            try
            {
                var existingPlan = await _context.MembershipPlans.FindAsync(Id);
                if (existingPlan == null)
                {
                    _logger.LogWarning(
                        "Attempted to delete non-existent membership plan with Id {PlanId}.",
                        Id
                    );
                    return NotFound(new { message = "Membership plan not found." });
                }

                _context.MembershipPlans.Remove(existingPlan);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Deactivated membership plan with Id {PlanId} for user {UserId}.",
                    Id,
                    GetAuthenticatedUserId()
                );
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating membership plan with Id {PlanId}.", Id);
                return StatusCode(500, new { message = "An error occurred while deactivating the plan." });
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession(
            [FromBody] SubscriptionRequestDto request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetAuthenticatedUserId();
            var planToSubscribe = await _context.MembershipPlans.FindAsync(request.PlanId);

            if (planToSubscribe == null || !planToSubscribe.IsActive)
            {
                _logger.LogWarning(
                    "PlanId {RequestedPlanId} not found or not active for UserId {UserId}.",
                    request.PlanId,
                    userId
                );
                return NotFound(new { message = "Selected plan is not valid or active." });
            }

            var existingActiveMembership = await _context.UserMemberships.FirstOrDefaultAsync(um =>
                um.UserId == userId
                && um.MembershipPlanId == request.PlanId
                && um.Status == MembershipStatusType.Active
                && um.EndDate > DateTime.UtcNow
            );

            if (existingActiveMembership != null)
            {
                _logger.LogInformation(
                    "User {UserId} already has an active subscription to PlanId {PlanId}.",
                    userId,
                    request.PlanId
                );
                return BadRequest(
                    new { message = "You already have an active subscription to this plan." }
                );
            }

            if (planToSubscribe.Price == 0)
            {
                var freeMembership = new UserMembershipModel
                {
                    UserId = userId,
                    MembershipPlanId = planToSubscribe.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = planToSubscribe.DurationInDays.HasValue
                        ? DateTime.UtcNow.AddDays(planToSubscribe.DurationInDays.Value)
                        : DateTime.UtcNow.AddYears(100),
                    Status = MembershipStatusType.Active,
                    AutoRenew = false,
                };
                _context.UserMemberships.Add(freeMembership);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Successfully subscribed User {UserId} to free PlanId {PlanId}. Membership ID: {MembershipId}",
                    userId,
                    freeMembership.MembershipPlanId,
                    freeMembership.Id
                );
                return Ok(
                    new
                    {
                        message = "Successfully subscribed to the free plan.",
                        userMembershipId = freeMembership.Id,
                        sessionId = (string?)null,
                        checkoutUrl = (string?)null,
                    }
                );
            }

            var newMembership = new UserMembershipModel
            {
                UserId = userId,
                MembershipPlanId = planToSubscribe.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(planToSubscribe.DurationInDays ?? 30),
                Status = MembershipStatusType.PendingPayment,
                AutoRenew = request.AutoRenew,
            };
            _context.UserMemberships.Add(newMembership);
            await _context.SaveChangesAsync();

            var newPayment = new PaymentModel
            {
                UserId = userId,
                UserMembershipId = newMembership.Id,
                Amount = planToSubscribe.Price,
                Currency = planToSubscribe.Currency ?? BaseCurrencyType.Error,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatusType.Pending,
            };
            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            var domain = $"{Request.Scheme}://{Request.Host}";
            var successUrl =
                $"{domain}/payment-success?session_id={{CHECKOUT_SESSION_ID}}&membership_id={newMembership.Id}";
            var cancelUrl = $"{domain}/payment-cancelled";

            var metadata = new Dictionary<string, string>
            {
                { "UserMembershipId", newMembership.Id.ToString() },
                { "PaymentId", newPayment.Id.ToString() },
                { "PlanId", planToSubscribe.Id.ToString() },
                { "UserId", userId.ToString() },
            };

            try
            {
                var session = await _paymentService.CreateCheckoutSessionAsync(
                    planToSubscribe.Price,
                    planToSubscribe.Currency.ToString() ?? "USD",
                    planToSubscribe.Name,
                    planToSubscribe.Description,
                    successUrl,
                    cancelUrl,
                    userId.ToString(),
                    metadata
                );

                if (session == null)
                {
                    _logger.LogError(
                        "PaymentService returned null for Checkout Session. UserId: {UserId}, PlanId: {PlanId}.",
                        userId,
                        planToSubscribe.Id
                    );
                    await MarkSubscriptionAsFailed(
                        newMembership.Id,
                        newPayment.Id,
                        "Failed to initiate Stripe session via PaymentService."
                    );
                    return StatusCode(500, new { message = "Could not initiate payment session." });
                }

                _logger.LogInformation(
                    "Stripe Checkout Session {SessionId} created for UserMembershipId {UserMembershipId}. User will be redirected.",
                    session.Id,
                    newMembership.Id
                );
                return Ok(new { sessionId = session.Id, checkoutUrl = session.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception creating Stripe Checkout Session. UserId: {UserId}, PlanId: {PlanId}.",
                    userId,
                    planToSubscribe.Id
                );
                await MarkSubscriptionAsFailed(
                    newMembership.Id,
                    newPayment.Id,
                    $"Error during Stripe session creation: {ex.Message}"
                );
                return BadRequest(
                    new
                    {
                        message = "An error occurred while initiating the payment: " + ex.Message,
                    }
                );
            }
        }

        private async Task MarkSubscriptionAsFailed(
            int membershipId,
            int paymentId,
            string gatewayResponseMessage
        )
        {
            var membership = await _context.UserMemberships.FindAsync(membershipId);
            var payment = await _context.Payments.FindAsync(paymentId);

            if (membership != null && membership.Status == MembershipStatusType.PendingPayment)
            {
                membership.Status = MembershipStatusType.FailedPayment;
            }
            if (payment != null && payment.Status == PaymentStatusType.Pending)
            {
                payment.Status = PaymentStatusType.Failed;
                payment.GatewayResponse = gatewayResponseMessage;
            }
            if (
                membership != null && _context.Entry(membership).State == EntityState.Modified
                || payment != null && _context.Entry(payment).State == EntityState.Modified
            )
            {
                await _context.SaveChangesAsync();
            }
        }

        [HttpPost("{userMembershipId}/cancel")]
        public async Task<IActionResult> CancelSubscription(int userMembershipId)
        {
            var userId = GetAuthenticatedUserId();
            var membershipToCancel = await _context.UserMemberships.FirstOrDefaultAsync(um =>
                um.Id == userMembershipId && um.UserId == userId
            );

            if (membershipToCancel == null)
            {
                return NotFound("Membership not found or you don't have permission to cancel it.");
            }

            if (membershipToCancel.Status != MembershipStatusType.Active)
            {
                return BadRequest("Only active memberships can be cancelled.");
            }

            membershipToCancel.AutoRenew = false;
            membershipToCancel.Status = MembershipStatusType.Cancelled;
            membershipToCancel.CancellationDate = DateTime.UtcNow;

            _context.Entry(membershipToCancel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    message = "Subscription cancellation requested. It will expire on "
                        + membershipToCancel.EndDate.ToShortDateString(),
                }
            );
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Dtos;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MembershipController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(MyDataContext context, ILogger<MembershipController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid in token for user {UserName}.", User.Identity?.Name ?? "Unknown");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }


        [HttpGet("current")]
        public async Task<ActionResult<UserMembershipDto>> GetCurrentUserActiveMembership()
        {
            var userId = GetAuthenticatedUserId();
            var activeMembership = await _context.UserMemberships
                .Include(um => um.Plan) // Plan bilgilerini de yükle
                .Where(um => um.UserId == userId && um.Status == MembershipStatus.Active && um.EndDate > DateTime.UtcNow)
                .OrderByDescending(um => um.EndDate) // En son aktif olan
                .Select(um => new UserMembershipDto
                {
                    UserMembershipId = um.UserMembershipId,
                    PlanId = um.UserMembershipId,
                    PlanName = um.Plan.Name, // Eager loading sayesinde erişilebilir
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew
                })
                .FirstOrDefaultAsync();

            if (activeMembership == null)
            {
                return NotFound("No active membership found.");
            }

            return Ok(activeMembership);
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
                    PlanId = um.UserMembershipId,
                    PlanName = um.Plan.Name,
                    StartDate = um.StartDate,
                    EndDate = um.EndDate,
                    Status = um.Status.ToString(),
                    AutoRenew = um.AutoRenew
                })
                .ToListAsync();

            return Ok(memberships);
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToPlan([FromBody] SubscriptionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetAuthenticatedUserId(); // Bunun doğru kullanıcı ID'sini (örn: 1) döndürdüğünü varsayıyoruz.
            _logger.LogInformation("SubscribeToPlan: Authenticated UserId = {UserIdFromToken}", userId);

            var planToSubscribe = await _context.MembershipPlans.FindAsync(request.PlanId);

            if (planToSubscribe == null || !planToSubscribe.IsActive)
            {
                _logger.LogWarning("SubscribeToPlan: Plan not found or not active for PlanId = {RequestedPlanId}", request.PlanId);
                return NotFound("Selected plan is not valid or active.");
            }
            _logger.LogInformation("SubscribeToPlan: Found Plan: ID={PlanId}, Name='{PlanName}'", planToSubscribe.MembershipPlanId, planToSubscribe.Name);


            // Kullanıcının mevcut aktif üyeliğini kontrol et
            var currentActiveMembership = await _context.UserMemberships
                .Where(um => um.UserId == userId && um.Status == MembershipStatus.Active && um.EndDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (currentActiveMembership != null)
            {
                if (currentActiveMembership.MembershipPlanId == request.PlanId) // Doğru karşılaştırma
                {
                    _logger.LogWarning("SubscribeToPlan: User {UserIdFromToken} is already subscribed to PlanId = {RequestedPlanId}", userId, request.PlanId);
                    return BadRequest("You are already subscribed to this plan.");
                }
                // TODO: Plan yükseltme/değiştirme mantığı
            }

            UserMembershipModel newMembership;

            if (planToSubscribe.Price == 0)
            {
                newMembership = new UserMembershipModel
                {
                    UserId = userId,
                    MembershipPlanId = planToSubscribe.MembershipPlanId, // DOĞRU: PlanId'yi ata
                                                                         // User ve Plan navigation property'leri ATANMIYOR (null kalacaklar)
                    StartDate = DateTime.UtcNow,
                    EndDate = planToSubscribe.DurationInDays.HasValue
                                ? DateTime.UtcNow.AddDays(planToSubscribe.DurationInDays.Value)
                                : DateTime.UtcNow.AddYears(100),
                    Status = MembershipStatus.Active,
                    AutoRenew = false
                };
                _context.UserMemberships.Add(newMembership);
                await _context.SaveChangesAsync();
                _logger.LogInformation("SubscribeToPlan: Successfully subscribed User {UserIdFromToken} to free PlanId = {PlanIdSubscribedTo}", userId, newMembership.MembershipPlanId);
                return Ok(new { message = "Successfully subscribed to the free plan.", userMembershipId = newMembership.UserMembershipId });
            }
            else
            {
                newMembership = new UserMembershipModel
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
                _logger.LogInformation("SaveChanges successful for UserMembership (Paid Plan), UserMembershipId = {GeneratedUserMembershipId}", newMembership.UserMembershipId);


                // Ödeme kaydı oluşturma (opsiyonel, bu ayrı bir SaveChanges gerektirebilir veya aynı transaction'da olabilir)
                var payment = new PaymentModel // PaymentModel'inizin tanımına göre ayarlayın
                {
                    UserId = userId,
                    UserMembershipId = newMembership.UserMembershipId, // Yeni oluşturulan üyelik ID'si
                    Amount = planToSubscribe.Price,
                    Currency = planToSubscribe.Currency,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending // PaymentStatus enum'ınızın adı neyse
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync(); // İkinci SaveChanges (isteğe bağlı, tek SaveChanges da olabilir)
                _logger.LogInformation("Payment record created with PaymentId = {GeneratedPaymentId} for UserMembershipId = {UserMembershipIdForPayment}", payment.PaymentId, newMembership.UserMembershipId);


                return Ok(new { message = "Subscription initiated. Please proceed to payment.", userMembershipId = newMembership.UserMembershipId, planPrice = planToSubscribe.Price, paymentId = payment.PaymentId });
            }
        }

        // POST: api/UserMemberships/{userMembershipId}/cancel
        // Bir üyeliği iptal etme (genellikle bir sonraki yenileme döneminde sonlanır)
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

            // Otomatik yenilemeyi kapat ve durumu güncelle
            membershipToCancel.AutoRenew = false;
            membershipToCancel.Status = MembershipStatus.Cancelled; // Veya farklı bir durum: "PendingCancellation"
            membershipToCancel.CancellationDate = DateTime.UtcNow;
            // EndDate genellikle aynı kalır, üyelik bu tarihe kadar devam eder.

            _context.Entry(membershipToCancel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subscription cancellation requested. It will expire on " + membershipToCancel.EndDate.ToShortDateString() });
        }

        // Ödeme başarılı olduktan sonra çağrılacak bir endpoint (Ödeme ağ geçidi webhook'u tarafından veya client tarafından)
        // Bu endpoint, UserMembership'i aktif hale getirmeli ve bir Payment kaydı oluşturmalı/güncellemeli.
        // Bu endpointin güvenliği çok önemli (webhook signature validation vb.)
        // Örnek:
        // [HttpPost("confirm-payment")]
        // [AllowAnonymous] // Veya özel bir API anahtarı ile korunmalı
        // public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto confirmation)
        // {
        //     // 1. Ödeme ağ geçidinden gelen isteği doğrula (signature, transactionId vb.)
        //     // 2. İlgili UserMembership'i bul (confirmation.UserMembershipId veya confirmation.OrderId ile)
        //     // 3. UserMembership'in Status'ünü Active yap, StartDate ve EndDate'i ayarla.
        //     // 4. Bir Payment kaydı oluştur (Succeeded status ile, TransactionId, Amount vb.)
        //     // await _context.SaveChangesAsync();
        //     // return Ok();
        // }
    }
}

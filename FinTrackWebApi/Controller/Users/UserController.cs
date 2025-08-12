using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Users
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly MyDataContext _context;

        public UserController(ILogger<UserController> logger, MyDataContext context)
        {
            _logger = logger;
            _context = context;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var user = await _context.Users
                    .Include(u => u.UserMemberships)
                        .ThenInclude(um => um.Plan)
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new UserProfileDto
                    {
                        // Temel Bilgiler
                        Id = u.Id,
                        UserName = u.UserName ?? u.NormalizedUserName ?? string.Empty,
                        Email = u.Email ?? u.NormalizedEmail ?? string.Empty,
                        ProfilePictureUrl = u.ProfilePicture ?? string.Empty,
                        CreatedAtUtc = u.CreatedAtUtc,

                        // Üyelik Bilgileri
                        CurrentMembershipPlanId = u.UserMemberships
                            .OrderByDescending(um => um.EndDate)
                            .Select(um => um.Plan.Id)
                            .FirstOrDefault(),
                        CurrentMembershipPlanType = (from um in u.UserMemberships
                                          orderby um.EndDate descending
                                          select um.Plan.Name)
                                .FirstOrDefault() ?? "No Membership",
                        MembershipStartDateUtc = u.UserMemberships
                            .OrderByDescending(um => um.EndDate)
                            .Select(um => um.StartDate)
                            .FirstOrDefault(),
                        MembershipExpirationDateUtc = u.UserMemberships
                            .OrderByDescending(um => um.EndDate)
                            .Select(um => um.EndDate)
                            .FirstOrDefault(),

                        // Ayarlar
                        Thema = u.AppSettings.Appearance ?? Enums.AppearanceType.Light,
                        Language = u.AppSettings.Language ?? Enums.LanguageType.en_EN,
                        Currency = u.AppSettings.BaseCurrency ?? Enums.BaseCurrencyType.USD,
                        SpendingLimitWarning = u.NotificationSettings.SpendingLimitWarning,
                        ExpectedBillReminder = u.NotificationSettings.ExpectedBillReminder,
                        WeeklySpendingSummary = u.NotificationSettings.WeeklySpendingSummary,
                        NewFeaturesAndAnnouncements = u.NotificationSettings.NewFeaturesAndAnnouncements,
                        EnableDesktopNotifications = u.NotificationSettings.EnableDesktopNotifications,


                        // Kullanımlar
                        CurrentAccounts = u.Accounts.Select(a => a.Id).ToList(),
                        CurrentBudgets = u.Budgets.Select(b => b.Id).ToList(),
                        CurrentTransactions = u.Transactions.Select(t => t.Id).ToList(),
                        CurrentBudgetsCategories = u.Budgets.Select(b => b.Category).Select(c => c.Id).ToList(),
                        CurrentTransactionsCategories = u.TransactionCategories.Select(tc => tc.Id).ToList(),
                        CurrentLenderDebts = u.DebtsAsLender.Select(d => d.Id).ToList(),
                        CurrentBorrowerDebts = u.DebtsAsBorrower.Select(d => d.Id).ToList(),
                        CurrentNotifications = u.Notifications.Select(n => n.Id).ToList(),
                        CurrentFeedbacks = u.Feedbacks.Select(f => f.Id).ToList(),
                        CurrentVideos = u.UploadedVideos.Select(v => v.Id).ToList()
                    })
                    .FirstOrDefaultAsync();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information for user ID: {UserId}", userId);
                return StatusCode(500, "Internal server error while retrieving user information.");
            }
        }
    }
}

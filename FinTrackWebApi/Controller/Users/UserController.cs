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
                        Id = u.Id,
                        UserName = u.UserName ?? u.NormalizedUserName ?? string.Empty,
                        Email = u.Email ?? u.NormalizedEmail ?? string.Empty,
                        ProfilePicture = u.ProfilePicture ?? string.Empty,
                        MembershipType = (from um in u.UserMemberships
                                orderby um.EndDate descending
                                select um.Plan.Name)
                                .FirstOrDefault() ?? "Üyelik Yok"
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

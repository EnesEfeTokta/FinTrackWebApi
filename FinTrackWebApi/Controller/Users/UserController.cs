using FinTrackWebApi.Data;
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
                    .Include(u => u.UserMemberships).AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                var userInfo = new
                {
                    userId,
                    user?.UserName,
                    user?.Email,
                    user?.ProfilePicture,
                    user?.UserMemberships,
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information for user ID: {UserId}", userId);
                return StatusCode(500, "Internal server error while retrieving user information.");
            }
        }
    }
}

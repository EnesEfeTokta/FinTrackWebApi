using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Users
{
    [ApiController]
    [Route("api/[controller]")]
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

        private string GetCurrentUserIdString()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "null";
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = GetCurrentUserIdString();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Failed to verify user identity.");
            }

            try
            {
                var userMembership = await _context.UserMemberships
                    .Include(um => um.Plan)
                    .FirstOrDefaultAsync(um => um.UserId.ToString() == userId);

                var userInfo = new
                {
                    UserId = userId,
                    UserName = User.Identity?.Name,
                    Email = User.FindFirstValue(ClaimTypes.Email),
                    Roles = User.FindAll(ClaimTypes.Role).Select(role => role.Value).ToList(),
                    ProfilePicture = User.FindFirstValue("ProfilePicture") ?? "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740",
                    MembershipController = userMembership?.Plan?.Name,
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

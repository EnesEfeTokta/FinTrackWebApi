using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserSettingsController : ControllerBase
    {
        private readonly MyDataContext _context;

        private readonly ILogger<UserSettingsController> _logger;

        public UserSettingsController(MyDataContext context, ILogger<UserSettingsController> logger)
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
                // Üretimde daha spesifik bir exception veya uygun bir dönüş yapılabilir.
                // Ancak [Authorize] düzgün çalışıyorsa bu noktaya gelinmemeli.
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSettings()
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            var userSettings = await _context.UserSettings
                 .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == authenticatedUserId);
            if (userSettings == null)
            {
                _logger.LogWarning("User settings not found for user ID: {UserId}", authenticatedUserId);
                return NotFound("User settings not found.");
            }

            _logger.LogInformation("Successfully retrieved settings for user ID: {UserId}", authenticatedUserId);
            return Ok(userSettings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserSettings([FromBody] UserSettingsDto userSettingsDto)
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            var userSettings = new UserSettingsModel
            {
                UserId = authenticatedUserId,
                Theme = userSettingsDto.Theme,
                Language = userSettingsDto.Language,
                Notification = userSettingsDto.Notification
            };
            _context.UserSettings.Add(userSettings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully created settings for user ID: {UserId}", authenticatedUserId);
            return CreatedAtAction(nameof(GetUserSettings), new { userId = userSettings.UserId }, userSettings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserSettings(int userId, [FromBody] UserSettingsDto userSettingsDto)
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == authenticatedUserId);
            if (userSettings == null)
            {
                return NotFound("User settings not found.");
            }
            userSettings.Theme = userSettingsDto.Theme;
            userSettings.Language = userSettingsDto.Language;
            userSettings.Notification = userSettingsDto.Notification;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated settings for user ID: {UserId}", authenticatedUserId);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserSettings()
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == authenticatedUserId);
            if (userSettings == null)
            {
                _logger.LogWarning("User settings not found for user ID: {UserId}", authenticatedUserId);
                return NotFound("User settings not found.");
            }
            _context.UserSettings.Remove(userSettings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted settings for user ID: {UserId}", authenticatedUserId);
            return NoContent();
        }
    }
}

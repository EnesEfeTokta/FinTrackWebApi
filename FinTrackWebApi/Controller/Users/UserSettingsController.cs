using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.UserSettingsDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Controller.Users
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class UserSettingsController : ControllerBase
    {
        private readonly MyDataContext _context;

        private readonly ILogger<UserSettingsController> _logger;

        public UserSettingsController(
            MyDataContext context,
            ILogger<UserSettingsController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError(
                    "Authenticated user ID claim (NameIdentifier) not found or invalid in token for user {UserName}.",
                    User.Identity?.Name ?? "Unknown"
                );
                throw new UnauthorizedAccessException(
                    "User ID cannot be determined from the token."
                );
            }
            return userId;
        }

        [HttpGet("AppSettings")]
        public async Task<IActionResult> GetAppSettings()
        {
            var userId = GetAuthenticatedUserId();
            try
            {
                var settings = await _context.UserAppSettings.AsNoTracking().Where(s => s.UserId == userId)
                    .Select(s => new UserAppSettingsDto
                    {
                        Id = s.Id,
                        Appearance = s.Appearance ?? AppearanceType.Dark,
                        Currency = s.BaseCurrency ?? BaseCurrencyType.Error,
                        CreatedAtUtc = s.CreatedAtUtc,
                        UpdatedAtUtc = s.UpdatedAtUtc

                    }).FirstOrDefaultAsync();
                if (settings == null)
                {
                    _logger.LogWarning(
                        "Application settings for user ID {UserId} could not be found.",
                        userId
                    );
                    return NotFound($"Application settings for user ID {userId} could not be found.");
                }

                _logger.LogInformation("Application settings found for user ID {userId}.", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Application settings for user ID {userId} could not be found.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Application settings for user ID not found.");
            }
        }

        [HttpPost("AppSettings")]
        public async Task<IActionResult> SetAppSettings([FromBody] UserAppSettingsUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UserAppSettings data is required.");
            }

            try
            {
                int userId = GetAuthenticatedUserId();
                var settings = await _context.UserAppSettings.FirstOrDefaultAsync(s => s.UserId == userId);
                if (settings == null)
                {
                    _logger.LogWarning(
                        "Application settings for user ID {UserId} could not be found.",
                        userId
                    );
                    return NotFound($"Application settings for user ID {userId} could not be found.");
                }

                settings.Appearance = updateDto.Appearance;
                settings.BaseCurrency = updateDto.Currency;
                settings.UpdatedAtUtc = DateTime.UtcNow;

                _context.UserAppSettings.Update(settings);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"The application settings for user ID {userId} have been updated.");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the application settings for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the application settings for the user ID.");
            }
        }

        [HttpGet("ProfileSettings")]
        public async Task<IActionResult> GetProfileSettings()
        {
            var userId = GetAuthenticatedUserId();
            try
            {
                var settings = await _context.Users.AsNoTracking().Where(s => s.Id == userId)
                    .Select(s => new ProfileSettingsDto
                    {
                        Id = s.Id,
                        FullName = s.UserName ?? "N/A",
                        Email = s.Email ?? "N/A",
                        ProfilePictureUrl = s.ProfilePicture,
                        CreatedAtUtc = s.CreatedAtUtc

                    }).FirstOrDefaultAsync();
                if (settings == null)
                {
                    _logger.LogWarning(
                        "User ID {userId} could not be found.",
                        userId
                    );
                    return NotFound($"User ID {userId} could not be found.");
                }

                _logger.LogInformation("Profile settings found for user ID {userId}.", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while retrieving profile settings for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving profile settings for user ID.");
            }
        }

        [HttpPost("ProfileSettings")]
        public async Task<IActionResult> SetProfileSettings([FromBody] ProfileSettingsUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("ProfileSettings data is required.");
            }

            try
            {
                int userId = GetAuthenticatedUserId();
                var settings = await _context.Users.FirstOrDefaultAsync(s => s.Id == userId);
                if (settings == null)
                {
                    _logger.LogWarning(
                        "User ID {userId} could not be found.",
                        userId
                    );
                    return NotFound($"User ID {userId} could not be found.");
                }

                if (updateDto.FullName != null)
                {
                    string UserName = updateDto.FullName.Replace(" ", "").Trim();
                    settings.UserName = updateDto.FullName;
                    settings.NormalizedUserName = UserName.ToUpper();
                }

                if (updateDto.Email != null)
                {
                    if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email.Contains("@") && updateDto.Email.Contains("."))
                    {
                        string Email = updateDto.Email.Replace(" ", "").Trim();
                        settings.Email = updateDto.Email;
                        settings.NormalizedEmail = Email.ToUpper();
                    }
                    else
                    {
                        return BadRequest("You did not enter your email address correctly.");
                    }
                }

                if (updateDto.ProfilePictureUrl != null)
                {
                    settings.ProfilePicture = updateDto.ProfilePictureUrl;
                }

                _context.Users.Update(settings);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile settings have been updated for user ID {userId}.");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the profile settings for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the profile settings for user ID.");
            }
        }

        //[HttpPost("UserPasswordSettings")]
        //public async Task<IActionResult> SetUserPasswordSettings([FromBody] UserPasswordSettingsUpdateDto updateDto)
        //{
        //    if (updateDto == null)
        //    {
        //        return BadRequest("UserPasswordSettings data is required.");
        //    }

        //    try
        //    {
        //        int userId = GetAuthenticatedUserId();
        //        var settings = await _context.Users.FirstOrDefaultAsync(s => s.Id == userId);
        //        if (settings == null)
        //        {
        //            _logger.LogWarning(
        //                "User ID {userId} could not be found.",
        //                userId
        //            );
        //            return NotFound($"User ID {userId} could not be found.");
        //        }

        //        if (updateDto.FullName != null)
        //        {
        //            string UserName = updateDto.FullName.Replace(" ", "").Trim();
        //            settings.UserName = updateDto.FullName;
        //            settings.NormalizedUserName = UserName.ToUpper();
        //        }

        //        if (updateDto.Email != null)
        //        {
        //            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email.Contains("@") && updateDto.Email.Contains("."))
        //            {
        //                string Email = updateDto.Email.Replace(" ", "").Trim();
        //                settings.Email = updateDto.Email;
        //                settings.NormalizedEmail = Email.ToUpper();
        //            }
        //            else
        //            {
        //                return BadRequest("You did not enter your email address correctly.");
        //            }
        //        }

        //        if (updateDto.ProfilePictureUrl != null)
        //        {
        //            settings.ProfilePicture = updateDto.ProfilePictureUrl;
        //        }

        //        _context.Users.Update(settings);
        //        await _context.SaveChangesAsync();

        //        _logger.LogInformation($"Profile settings have been updated for user ID {userId}.");
        //        return Ok(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "An error occurred while updating the profile settings for user ID {UserId}.",
        //            GetAuthenticatedUserId()
        //        );
        //        return StatusCode(500, "An error occurred while updating the profile settings for user ID.");
        //    }
        //}

        [HttpGet("UserNotificationSettings")]
        public async Task<IActionResult> GetUserNotificationSettings()
        {
            var userId = GetAuthenticatedUserId();
            try
            {
                var settings = await _context.UserNotificationSettings.AsNoTracking().Where(s => s.UserId == userId)
                    .Select(s => new UserNotificationSettingsDto
                    {
                        Id = s.Id,
                        SpendingLimitWarning = s.SpendingLimitWarning,
                        ExpectedBillReminder = s.ExpectedBillReminder,
                        WeeklySpendingSummary = s.WeeklySpendingSummary,
                        NewFeaturesAndAnnouncements = s.NewFeaturesAndAnnouncements,
                        EnableDesktopNotifications = s.EnableDesktopNotifications,
                        CreatedAtUtc = s.CreatedAtUtc,
                        UpdatedAtUtc = s.UpdatedAtUtc

                    }).FirstOrDefaultAsync();
                if (settings == null)
                {
                    _logger.LogWarning(
                        "Notification settings for user ID {UserId} could not be found.",
                        userId
                    );
                    return NotFound($"Notification settings for user ID {userId} could not be found.");
                }

                _logger.LogInformation("Notification settings found for user ID {userId}.", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Notification settings for user ID {userId} could not be found.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Notification settings for user ID not found.");
            }
        }

        [HttpPost("UserNotificationSettings")]
        public async Task<IActionResult> SetUserNotificationSettings([FromBody] UserNotificationSettingsUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UserAppSettings data is required.");
            }

            try
            {
                int userId = GetAuthenticatedUserId();
                var settings = await _context.UserNotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId);
                if (settings == null)
                {
                    _logger.LogWarning(
                        "Notification settings for user ID {UserId} could not be found.",
                        userId
                    );
                    return NotFound($"Notification settings for user ID {userId} could not be found.");
                }

                settings.SpendingLimitWarning = updateDto.SpendingLimitWarning;
                settings.ExpectedBillReminder = updateDto.ExpectedBillReminder;
                settings.WeeklySpendingSummary = updateDto.WeeklySpendingSummary;
                settings.NewFeaturesAndAnnouncements = updateDto.NewFeaturesAndAnnouncements;
                settings.EnableDesktopNotifications = updateDto.EnableDesktopNotifications;
                settings.UpdatedAtUtc = DateTime.UtcNow;

                _context.UserNotificationSettings.Update(settings);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"The Notification settings for user ID {userId} have been updated.");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the notification settings for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the notification settings for the user ID.");
            }
        }
    }
}
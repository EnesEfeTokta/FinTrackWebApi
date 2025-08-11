using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.UserSettingsDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.User;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Users
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class UserSettingsController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<UserSettingsController> _logger;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserSettingsController(
            MyDataContext context,
            ILogger<UserSettingsController> logger,
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager,
            IOtpService otpService,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<(UserModel? User, int UserId, IActionResult? ErrorResult)> GetAuthenticatedUserAndIdAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("User ID claim not found or invalid in token.");
                return (null, 0, Unauthorized("User ID cannot be determined or is invalid."));
            }

            var user = await _userManager.FindByIdAsync(userIdString);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found in database.", userId);
                return (null, userId, NotFound($"User with ID {userId} not found."));
            }

            return (user, userId, null);
        }

        [HttpPost("update-username")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameDto updateDto)
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                string newUserName = $"{updateDto.FirstName.Trim()}_{updateDto.LastName.Trim()}";

                var existingUser = await _userManager.FindByNameAsync(newUserName);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning("Username {UserName} is already taken by another user.", newUserName);
                    return Conflict($"Username '{newUserName}' is already taken.");
                }

                var result = await _userManager.SetUserNameAsync(user!, newUserName);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update username for user ID {UserId}. Errors: {Errors}", userId, result.Errors);
                    return BadRequest(new { Message = "Failed to update username.", Errors = result.Errors });
                }

                _logger.LogInformation("Username updated successfully for user ID {UserId}. New username: {NewUserName}", userId, newUserName);
                return Ok(new { Message = "Username updated successfully.", NewUserName = newUserName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating username for user ID {UserId}.", userId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange()
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            if (user?.Email == null)
            {
                return BadRequest("User does not have a registered email address.");
            }

            try
            {
                string otp = _otpService.GenerateOtp();
                string hashedOtp = BCrypt.Net.BCrypt.HashPassword(otp);
                DateTime expiryTime = DateTime.UtcNow.AddMinutes(15);

                await _otpService.StoreOtpAsync(user.Email, hashedOtp, string.Empty, string.Empty, string.Empty, expiryTime);

                string emailSubject = "New Email Change Request";
                string emailBody;
                string emailTemplatePath = Path.Combine(
                    _webHostEnvironment.ContentRootPath, "Services", "EmailService", "EmailHtmlSchemes", "EmailChangeConfirmationCodeScheme.html");

                if (!System.IO.File.Exists(emailTemplatePath))
                {
                    _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                    await _otpService.RemoveOtpAsync(user.Email);
                    return StatusCode(500, new { Message = "Email template not found." });
                }
                using (var reader = new StreamReader(emailTemplatePath))
                {
                    emailBody = await reader.ReadToEndAsync();
                }
                emailBody = emailBody.Replace("[UserName]", user.UserName);
                emailBody = emailBody.Replace("[CONFIRMATION_CODE]", otp);
                emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);

                _logger.LogInformation("OTP for email change sent to {Email} for user ID {UserId}.", user.Email, userId);
                return Ok(new { Message = "An OTP has been sent to your current email address to verify your identity." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}.", user.Email);
                await _otpService.RemoveOtpAsync(user.Email);
                return StatusCode(500, new { Message = "Failed to send verification email. Please try again." });
            }
        }

        [HttpPost("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromBody] UpdateUserEmailDto updateDto)
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            if (string.IsNullOrEmpty(user!.Email))
            {
                return BadRequest("User does not have a current email address.");
            }

            try
            {
                var isOtpValid = await _otpService.VerifyOtpAsync(user.Email, updateDto.OtpCode);
                if (isOtpValid == null)
                {
                    return BadRequest("Invalid or expired OTP.");
                }

                var existingUserWithNewEmail = await _userManager.FindByEmailAsync(updateDto.NewEmail);
                if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != userId)
                {
                    return Conflict($"Email '{updateDto.NewEmail}' is already in use.");
                }

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, updateDto.NewEmail);
                var result = await _userManager.ChangeEmailAsync(user, updateDto.NewEmail, token);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update email for user ID {UserId}. Errors: {Errors}", userId, result.Errors);
                    return BadRequest(new { Message = "Failed to update email.", Errors = result.Errors });
                }

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                await _otpService.RemoveOtpAsync(user.Email);

                _logger.LogInformation("Email updated successfully for user ID {UserId} to {NewEmail}", userId, updateDto.NewEmail);
                return Ok(new { Message = "Your email has been successfully updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming email change for user ID {UserId}.", userId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("update-profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UpdateProfilePictureDto data is required.");
            }

            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                user!.ProfilePicture = updateDto.ProfilePictureUrl;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update profile picture for user ID {UserId}. Errors: {Errors}", userId, result.Errors);
                    return BadRequest(new { Message = "Failed to update profile picture.", Errors = result.Errors });
                }

                _logger.LogInformation($"Profile picture updated successfully for user ID {userId}.");
                return Ok(new { Message = "Profile picture updated successfully.", ProfilePictureUrl = updateDto.ProfilePictureUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the profile picture for user ID {UserId}.", userId);
                return StatusCode(500, "An error occurred while updating the profile picture.");
            }
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUserPasswordDto updateDto)
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _userManager.ChangePasswordAsync(user!, updateDto.CurrentPassword, updateDto.NewPassword);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password change failed for user ID {UserId}.", userId);
                    return BadRequest(new { Message = "Password change failed. Please check your current password and try again.", Errors = result.Errors });
                }

                _logger.LogInformation("Password updated successfully for user ID {UserId}.", userId);
                return Ok(new { Message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating password for user ID {UserId}.", userId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("app-settings")]
        public async Task<IActionResult> GetAppSettings()
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var settings = await _context.UserAppSettings.AsNoTracking()
                    .Where(s => s.UserId == userId)
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
                    _logger.LogWarning("Application settings for user ID {UserId} could not be found.", userId);
                    return NotFound($"Application settings for user ID {userId} could not be found.");
                }

                _logger.LogInformation("Application settings found for user ID {userId}.", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting app settings for user ID {userId}.", userId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("app-settings")]
        public async Task<IActionResult> SetAppSettings([FromBody] UserAppSettingsUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UserAppSettings data is required.");
            }

            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var settings = await _context.UserAppSettings.FirstOrDefaultAsync(s => s.UserId == userId);
                if (settings == null)
                {
                    _logger.LogWarning("Application settings for user ID {UserId} could not be found.", userId);
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
                _logger.LogError(ex, "An error occurred while updating the application settings for user ID {UserId}.", userId);
                return StatusCode(500, "An error occurred while updating the application settings.");
            }
        }

        [HttpGet("user-notification-settings")]
        public async Task<IActionResult> GetUserNotificationSettings()
        {
            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var settings = await _context.UserNotificationSettings.AsNoTracking()
                    .Where(s => s.UserId == userId)
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
                    _logger.LogWarning("Notification settings for user ID {UserId} could not be found.", userId);
                    return NotFound($"Notification settings for user ID {userId} could not be found.");
                }

                _logger.LogInformation("Notification settings found for user ID {userId}.", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification settings for user ID {userId} could not be found.", userId);
                return StatusCode(500, "Notification settings for user ID not found.");
            }
        }

        [HttpPost("user-notificationettings")]
        public async Task<IActionResult> SetUserNotificationSettings([FromBody] UserNotificationSettingsUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UserNotificationSettings data is required.");
            }

            var (user, userId, errorResult) = await GetAuthenticatedUserAndIdAsync();
            if (errorResult != null) return errorResult;

            try
            {
                var settings = await _context.UserNotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId);
                if (settings == null)
                {
                    _logger.LogWarning("Notification settings for user ID {UserId} could not be found.", userId);
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
                _logger.LogError(ex, "An error occurred while updating the notification settings for user ID {UserId}.", userId);
                return StatusCode(500, "An error occurred while updating the notification settings.");
            }
        }
    }
}
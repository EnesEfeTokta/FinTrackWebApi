using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.AuthDtos;
using FinTrackWebApi.Dtos.UserSettingsDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace FinTrackWebApi.Controller.Users
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class UserSettingsController : ControllerBase
    {
        private readonly MyDataContext _context;

        private readonly ILogger<UserSettingsController> _logger;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserSettingsController(
            MyDataContext context,
            ILogger<UserSettingsController> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IOtpService otpService,
            IWebHostEnvironment webHostEnvironment

        )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpPost("update-username")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UpdateUserNameDto data is required.");
            }
            var userId = GetAuthenticatedUserId();
            try
            {
                string newUserName = updateDto.FirstName.Replace(" ", "").Trim() + "_" + updateDto.LastName.Replace(" ", "").Trim();

                var existingUser = await _userManager.FindByNameAsync(newUserName);
                if (existingUser != null && existingUser.Id != userId.ToString())
                {
                    _logger.LogWarning(
                        "Username {UserName} is already taken by another user.",
                        newUserName
                    );
                    return BadRequest($"Username {newUserName} is already taken.");
                }

                var result = await _userManager.SetUserNameAsync(
                    await _userManager.FindByIdAsync(userId.ToString()),
                    newUserName
                );
                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Failed to update username for user ID {UserId}. Errors: {Errors}",
                        userId,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                    return BadRequest("Failed to update username. Please try again.");
                }
                _logger.LogInformation(
                    "Username updated successfully for user ID {UserId}. New username: {NewUserName}",
                    userId,
                    newUserName
                );
                return Ok(new { Message = "Username updated successfully.", NewUserName = newUserName });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the username for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the username for the user ID.");
            }
        }

        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange()
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.Email == null)
                {
                    return NotFound("User or user email not found.");
                }

                string otp = _otpService.GenerateOtp();
                DateTime expiryTime = DateTime.UtcNow.AddMinutes(15);

                await _otpService.StoreOtpAsync(user.Email, otp, string.Empty, string.Empty, string.Empty, expiryTime);

                try
                {
                    string emailSubject = "New Email Change Request";
                    string emailBody = string.Empty;
                    string emailTemplatePath = Path.Combine(
                        _webHostEnvironment.ContentRootPath,
                        "Services",
                        "EmailService",
                        "EmailHtmlSchemes",
                        "EmailChangeConfirmationCodeScheme.html"
                    );

                    if (!System.IO.File.Exists(emailTemplatePath))
                    {
                        _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                        await _otpService.RemoveOtpAsync(user.Email);
                        return StatusCode(500, new { Message = "Email template not found." });
                    }
                    using (StreamReader reader = new StreamReader(emailTemplatePath))
                    {
                        emailBody = await reader.ReadToEndAsync();
                    }
                    emailBody = emailBody.Replace("[UserName]", user.UserName);
                    emailBody = emailBody.Replace("[CONFIRMATION_CODE]", otp);
                    emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                    await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send verification email to {Email}.",
                        user.Email
                    );
                    await _otpService.RemoveOtpAsync(user.Email);
                    return StatusCode(
                        500,
                        new
                        {
                            Message = "Failed to send verification email. Please check the address and try again.",
                        }
                    );
                }

                _logger.LogInformation("OTP for email change sent to {Email} for user ID {UserId}.", user.Email, userId);
                return Ok(new { Message = "An OTP has been sent to your current email address to verify your identity." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while requesting an email change OTP.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromBody] UpdateUserEmailDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetAuthenticatedUserId();

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.Email == null)
                {
                    return NotFound("User not found.");
                }

                var isOtpValid = await _otpService.VerifyOtpAsync(user.Email, updateDto.OtpCode);
                if (isOtpValid == null)
                {
                    return BadRequest("Invalid or expired OTP.");
                }

                var existingUser = await _userManager.FindByEmailAsync(updateDto.NewEmail);
                if (existingUser != null && existingUser.Id != userId.ToString())
                {
                    return Conflict($"Email '{updateDto.NewEmail}' is already in use.");
                }

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, updateDto.NewEmail);
                var result = await _userManager.ChangeEmailAsync(user, updateDto.NewEmail, token);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update email for user ID {UserId}. Errors: {Errors}", userId, result.Errors);
                    return BadRequest(result.Errors);
                }

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Email updated successfully for user ID {UserId} to {NewEmail}", userId, updateDto.NewEmail);
                return Ok(new { Message = "Your email has been successfully updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming the email change.");
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
            try
            {
                int userId = GetAuthenticatedUserId();

                var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning(
                        "User ID {userId} could not be found.",
                        userId
                    );
                    return NotFound($"User ID {userId} could not be found.");
                }

                user.ProfilePicture = updateDto.ProfilePictureUrl;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile picture updated successfully for user ID {userId}.");
                return Ok(new { Message = "Profile picture updated successfully.", ProfilePictureUrl = updateDto.ProfilePictureUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the profile picture for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the profile picture for the user ID.");
            }
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUserPasswordDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("UpdateUserPasswordDto data is required.");
            }
            try
            {
                int userId = GetAuthenticatedUserId();

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning(
                        "User ID {userId} could not be found.",
                        userId
                    );
                    return NotFound($"User ID {userId} could not be found.");
                }

                var currentPasswordVerification = await _signInManager.CheckPasswordSignInAsync(user, updateDto.CurrentPassword, lockoutOnFailure: true);
                if (!currentPasswordVerification.Succeeded)
                {
                    _logger.LogWarning(
                        "Current password verification failed for user ID {UserId}.",
                        userId
                    );
                    return BadRequest("Current password is incorrect.");
                }

                var result = await _userManager.ChangePasswordAsync(user, updateDto.CurrentPassword, updateDto.NewPassword);

                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Failed to update password for user ID {UserId}. Errors: {Errors}",
                        userId,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                    return BadRequest("Failed to update password. Please try again.");
                }

                _logger.LogInformation($"Password updated successfully for user ID {userId}.");
                return Ok(new { Message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the password for user ID {UserId}.",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the password for the user ID.");
            }
        }






        [HttpGet("app-settings")]
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

        [HttpPost("app-settings")]
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

        [HttpGet("profile-settings")]
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

        [HttpGet("user-notification-settings")]
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

        [HttpPost("user-notificationettings")]
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
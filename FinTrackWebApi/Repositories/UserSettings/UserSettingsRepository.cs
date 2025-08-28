using FinTrackWebApi.Data;
using FinTrackWebApi.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Repositories.UserSettings
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly MyDataContext _context;
        private readonly ILogger<UserSettingsRepository> _logger;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public UserSettingsRepository(
            MyDataContext context,
            ILogger<UserSettingsRepository> logger,
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task AddAppSettingsAsync(UserAppSettingsModel settings)
        {
            await _context.UserAppSettings.AddAsync(settings);
        }

        public async Task AddDashboardSettingsAsync(UserDashboardSettingsModel settings)
        {
            await _context.UserDashboardSettings.AddAsync(settings);
        }

        public async Task AddNotificationSettingsAsync(UserNotificationSettingsModel settings)
        {
            await _context.UserNotificationSettings.AddAsync(settings);
        }

        public async Task<UserAppSettingsModel?> GetAppSettingsAsync(int userId)
        {
            return await _context.UserAppSettings
                .FirstOrDefaultAsync(ap => ap.UserId == userId);
        }

        public async Task<UserDashboardSettingsModel?> GetDashboardSettingsAsync(int userId)
        {
            return await _context.UserDashboardSettings
                .FirstOrDefaultAsync(ud => ud.UserId == userId);
        }

        public async Task<UserNotificationSettingsModel?> GetNotificationSettingsAsync(int userId)
        {
            return await _context.UserNotificationSettings
                .FirstOrDefaultAsync (un => un.UserId == userId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void UpdateAppSettings(UserAppSettingsModel settings)
        {
            _context.UserAppSettings.Update(settings);
        }

        public void UpdateDashboardSettings(UserDashboardSettingsModel settings)
        {
            _context.UserDashboardSettings.Update(settings);
        }

        public async Task UpdateEmail(UserModel user, string newEmail)
        {
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update email for user ID {UserId}. Errors: {Errors}", user.Id, result.Errors);
                return;
            }
        }

        public void UpdateNotificationSettings(UserNotificationSettingsModel settings)
        {
            _context.UserNotificationSettings.Update(settings);
        }

        public async Task UpdatePassword(UserModel user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user!, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update password for user ID {UserId}. Errors: {Errors}", user.Id, result.Errors);
                return;
            }
        }

        public async Task UpdateProfilePicture(UserModel user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update profile picture for user ID {UserId}. Errors: {Errors}", user.Id, result.Errors);
                return;
            }
        }

        public async Task UpdateUserName(UserModel user, string newUserName)
        {
            var result = await _userManager.SetUserNameAsync(user!, newUserName);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update username for user ID {UserId}. Errors: {Errors}", user.Id, result.Errors);
                return;
            }
        }
    }
}

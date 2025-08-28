using FinTrackWebApi.Dtos.UserSettingsDtos;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Repositories.UserSettings
{
    public interface IUserSettingsRepository
    {
        // -- App Settings --
        Task<UserAppSettingsModel?> GetAppSettingsAsync(int userId);
        Task AddAppSettingsAsync(UserAppSettingsModel settings);
        void UpdateAppSettings(UserAppSettingsModel settings);

        // -- Notification Settings --
        Task<UserNotificationSettingsModel?> GetNotificationSettingsAsync(int userId);
        Task AddNotificationSettingsAsync(UserNotificationSettingsModel settings);
        void UpdateNotificationSettings(UserNotificationSettingsModel settings);

        // -- Dashboard Settings --
        Task<UserDashboardSettingsModel?> GetDashboardSettingsAsync(int userId);
        Task AddDashboardSettingsAsync(UserDashboardSettingsModel settings);
        void UpdateDashboardSettings(UserDashboardSettingsModel settings);

        // -- User Settings --
        Task UpdateUserName(UserModel user, string newUserName);
        Task UpdatePassword(UserModel user, string currentPassword, string newPassword);
        Task UpdateProfilePicture(UserModel user);
        Task UpdateEmail(UserModel user, string newEmail);

        Task<bool> SaveChangesAsync();
    }
}

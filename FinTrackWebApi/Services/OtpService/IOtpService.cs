using FinTrackWebApi.Models;

namespace FinTrackWebApi.Services.OtpService
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> StoreOtpAsync(
            string email,
            string otpCodeHash,
            string username,
            string temporaryPlainPassword,
            string? profilePicture,
            DateTime expireAt
        );
        Task<OtpVerificationModel?> VerifyOtpAsync(string email, string plainOtpCode);
        Task<bool> RemoveOtpAsync(string email);
    }
}

using FinTrackWebApi.Models;

namespace FinTrackWebApi.Services.OtpService
{
    public interface IOtpService
    {
        string GenerateOtp();

        // Üretilen OTP'yi, hashlenmiş şifreyi ve diğer bilgileri geçici olarak saklar.
        Task<bool> StoreOtpAsync(string email, string hashOtpCde, string username, string passwordHash, string? profilePicture);

        // Sağlanan OTP kodunu belirli bir e-posta için doğrular.
        Task<OtpVerificationModel> VerifyOtpAsync(string email, string hashOtpCde);

        // Sağlanan OTP kodunu belirli bir e-posta için doğruladıktan sonra siler.
        Task<bool> RemoveOtpAsync(string email);
    }
}

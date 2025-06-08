// FinTrackWebApi.Services.OtpService.OtpService.cs
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using Microsoft.Extensions.Logging; // ILogger için

namespace FinTrackWebApi.Services.OtpService
{
    public class OtpService : IOtpService
    {
        private readonly MyDataContext _context;
        private readonly ILogger<OtpService> _logger;

        public OtpService(MyDataContext context, ILogger<OtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GenerateOtp()
        {
            // ... (GenerateOtp metodunuz aynı kalabilir) ...
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                uint otp = BitConverter.ToUInt32(bytes, 0) % 1000000;
                return otp.ToString("D6");
            }
        }

        public async Task<bool> StoreOtpAsync(string email, string otpCodeHash, string username, string temporaryPlainPassword, string? profilePicture, DateTime expireAt)
        {
            try
            {
                // Önceki OTP'leri temizle
                var existingOtps = await _context.OtpVerification
                    .Where(x => x.Email == email)
                    .ToListAsync();

                if (existingOtps.Any())
                {
                    _context.OtpVerification.RemoveRange(existingOtps);
                }

                OtpVerificationModel otpVerification = new OtpVerificationModel
                {
                    Email = email,
                    OtpCode = otpCodeHash, // Hash'lenmiş OTP
                    CreateAt = DateTime.UtcNow,
                    ExpireAt = expireAt,
                    Username = username,
                    TemporaryPlainPassword = temporaryPlainPassword, // Düz şifre
                    ProfilePicture = profilePicture ?? "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740"
                };

                await _context.OtpVerification.AddAsync(otpVerification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Storing OTP for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing OTP for email: {Email}", email);
                return false;
            }
        }

        public async Task<OtpVerificationModel?> VerifyOtpAsync(string email, string plainOtpCode)
        {
            try
            {
                var otpRecord = await _context.OtpVerification
                    .Where(x => x.Email == email)
                    .OrderByDescending(x => x.CreateAt)
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                {
                    _logger.LogWarning("No OTP record found for email: {Email}", email);
                    return null;
                }

                if (otpRecord.ExpireAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP expired for email: {Email}. ExpireAt: {ExpireAt}, UtcNow: {UtcNow}", email, otpRecord.ExpireAt, DateTime.UtcNow);
                    // Süresi dolmuş OTP'yi sil
                    _context.OtpVerification.Remove(otpRecord);
                    await _context.SaveChangesAsync();
                    return null;
                }

                // Gelen düz OTP'yi OtpVerificationModel'deki hash'lenmiş OTP ile karşılaştır
                if (!BCrypt.Net.BCrypt.Verify(plainOtpCode, otpRecord.OtpCode))
                {
                    _logger.LogWarning("Invalid OTP for email: {Email}. Provided OTP: {PlainOtpCode}, Stored Hash: {StoredHash}", email, plainOtpCode, otpRecord.OtpCode);
                    return null;
                }

                _logger.LogInformation("OTP verified for email: {Email}", email);
                // OTP doğrulandıktan sonra kaydı silmeyin, AuthController silecek.
                // Modeli döndürerek AuthController'ın içindeki verilere (TemporaryPlainPassword dahil) erişmesini sağlayın.
                return otpRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> RemoveOtpAsync(string email)
        {
            // ... (RemoveOtpAsync metodunuz aynı kalabilir) ...
            try
            {
                var existingOtps = await _context.OtpVerification
                                    .Where(x => x.Email == email)
                                    .ToListAsync();
                if (!existingOtps.Any()) return true; // Silinecek bir şey yoksa başarılı say

                _context.OtpVerification.RemoveRange(existingOtps);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed OTP(s) for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing OTP for email: {Email}", email);
                return false;
            }
        }
    }
}
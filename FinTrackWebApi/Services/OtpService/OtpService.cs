using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;

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
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);

                uint otp = BitConverter.ToUInt32(bytes, 0) % 1000000;
                return otp.ToString("D6");
            }
        }

        public async Task<bool> RemoveOtpAsync(string email)
        {
            try
            {
                var existingOtps = await _context.OtpVerifications
                                    .Where(x => x.Email == email)
                                    .ToListAsync();

                if (!existingOtps.Any())
                {
                    _logger.LogWarning("No OTP records found for email: {Email}", email);
                    return true;
                }

                _context.OtpVerifications.RemoveRange(existingOtps);
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

        public async Task<bool> StoreOtpAsync(string email, string hashOtpCde)
        {
            try
            {
                var existingOtps = await _context.OtpVerifications
                    .Where(x => x.Email == email)
                    .ToListAsync();

                if (existingOtps.Any())
                {
                    _logger.LogInformation("Removing OTP(s) for email: {Email}", email);
                    _context.OtpVerifications.RemoveRange(existingOtps);
                    await _context.SaveChangesAsync();
                }

                OtpVerifications otpVerification = new OtpVerifications
                {
                    Email = email,
                    OtpCode = hashOtpCde,
                    IsVerified = false,
                    CreateAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(5)
                };

                await _context.OtpVerifications.AddAsync(otpVerification);
                _logger.LogInformation("Storing OTP for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing OTP for email: {Email}", email);
                return false;
            }
        }

        public async Task<string> VerifyOtpAsync(string email, string hashOtpCde)
        {
            OtpVerifications otpVerifications = new OtpVerifications();

            try
            {
                var otpRecord = await _context.OtpVerifications
                    .Where(x => x.Email == email)
                    .OrderByDescending(x => x.CreateAt) // Sıralıyr ve en son eklenen kaydı alıyoruz.
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                {
                    _logger.LogWarning("No OTP record found for email: {Email}", email);
                    return "No OTP record found";
                }

                if (otpRecord.IsVerified)
                {
                    _logger.LogWarning("OTP already verified for email: {Email}", email);
                    return "Already Verified";
                }

                if (otpRecord.ExpireAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP expired for email: {Email}", email);
                    return "Expired";
                }

                if (otpRecord.OtpCode != hashOtpCde)
                {
                    _logger.LogWarning("Invalid OTP for email: {Email}", email);
                    return "Invalid OTP";
                }

                otpRecord.IsVerified = true;
                _logger.LogInformation("OTP verified for email: {Email}", email);

                return "Verified";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email: {Email}", email);
                return "Error";
            }
        }
    }
}

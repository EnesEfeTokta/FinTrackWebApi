using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Dtos;

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

        /// <summary>
        /// Verilen e-posta adresi için OTP'yi siler.
        /// </summary>
        /// <param name="email">Silinecek ilgili e-psta.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gönderilen e-posta adresi için OTP'yi saklar.
        /// </summary>
        /// <param name="email">Gönderilecek e-posta.</param>
        /// <param name="hashOtpCde">Gönderilecek kod.</param>
        /// <returns></returns>
        public async Task<bool> StoreOtpAsync(string email, string hashOtpCde, string username, string passwordHash, string? profilePicture)
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
                    //await _context.SaveChangesAsync();
                }

                OtpVerificationModel otpVerification = new OtpVerificationModel
                {
                    Email = email,
                    OtpCode = hashOtpCde,
                    CreateAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(5),
                    Username = username,
                    PasswordHash = passwordHash,
                    ProfilePicture = profilePicture ?? "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740"
                };

                await _context.OtpVerifications.AddAsync(otpVerification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Storing OTP for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing OTP for email: {Email}", email);
                return false;
            }
        }

        public async Task<OtpVerificationModel> VerifyOtpAsync(string email, string hashOtpCde)
        {
            OtpVerificationModel otpVerifications = new OtpVerificationModel();

            try
            {
                var allOtps = await _context.OtpVerifications.ToListAsync();
                _logger.LogInformation("Toplam OTP kaydı: {Count}", allOtps.Count);

                var otpRecord = await _context.OtpVerifications
                    .Where(x => x.Email == email)
                    .OrderByDescending(x => x.CreateAt) // Sıralıyr ve en son eklenen kaydı alıyoruz.
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                {
                    _logger.LogWarning("No OTP record found for email: {Email}", email);
                }

                if (otpRecord.ExpireAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP expired for email: {Email}", email);
                    _context.OtpVerifications.Remove(otpRecord);
                    await _context.SaveChangesAsync();
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(hashOtpCde, otpRecord.OtpCode))
                {
                    _logger.LogWarning("Invalid OTP for email: {Email} {OtpCode} {hashOtpCode}", email, otpRecord.OtpCode, hashOtpCde);
                    return null;
                }

                _logger.LogInformation("OTP verified for email: {Email}", email);

                // OTP doğrulandıktan sonra kaydı silmek isteyebilirsiniz.
                _context.OtpVerifications.Remove(otpRecord);
                await _context.SaveChangesAsync();

                return otpRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email: {Email}", email);
            }

            return null;
        }
    }
}

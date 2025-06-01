using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Security;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDataContext _context;

        private readonly IConfiguration _configuration;

        private readonly IEmailSender _emailSender;
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthController(
            MyDataContext context,
            IConfiguration configuration,
            IOtpService otpService,
            IEmailSender emailService,
            ILogger<AuthController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _otpService = otpService;
            _emailSender = emailService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("user/initiate-registration")]
        public async Task<IActionResult> UserInitiateRegistration([FromBody] InitiateRegistrationDto initiateRegistrationDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == initiateRegistrationDto.Email))
            {
                _logger.LogWarning("Registration initiation failed: Email {Email} already exists.", initiateRegistrationDto.Email);
                return BadRequest("This email address is already registered.");
            }

            string otp = _otpService.GenerateOtp();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(10);
            string hashOtpCde = BCrypt.Net.BCrypt.HashPassword(otp);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(initiateRegistrationDto.PasswordHash);

            bool stored = await _otpService.StoreOtpAsync(initiateRegistrationDto.Email, hashOtpCde, initiateRegistrationDto.Username, passwordHash, initiateRegistrationDto.ProfilePicture);

            try
            {
                string emailSubject = "Email Verification Code For FinTrack new Membership";
                string emailBody = string.Empty;

                string emailTemplatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Services", "EmailService", "EmailHtmlSchemes", "CodeVerificationScheme.html");
                if (!System.IO.File.Exists(emailTemplatePath))
                {
                    _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                    await _otpService.RemoveOtpAsync(initiateRegistrationDto.Email);
                    return StatusCode(500, "Email template not found.");
                }

                using (StreamReader reader = new StreamReader(emailTemplatePath))
                {
                    emailBody = await reader.ReadToEndAsync();
                }

                emailBody = emailBody.Replace("[UserName]", initiateRegistrationDto.Username);
                emailBody = emailBody.Replace("[VERIFICATION_CODE]", otp);

                emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                await _emailSender.SendEmailAsync(initiateRegistrationDto.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}.", initiateRegistrationDto.Email);

                // E-posta gönderilemezse, oluşturulan geçici OTP kaydını temizle!
                await _otpService.RemoveOtpAsync(initiateRegistrationDto.Email);
                return StatusCode(500, "Failed to send verification email. Please check the address and try again.");
            }

            _logger.LogInformation("OTP sent to {Email}.", initiateRegistrationDto.Email);
            return Ok(new { Message = "OTP sent to your email address." });
        }

        // OTP doğrulama işlemi yapılıyor.
        [HttpPost("user/verify-otp")]
        public async Task<IActionResult> UserVerifyOtp([FromBody] VerifyOtpRequestDto verifyOtpDto)
        {
            OtpVerificationModel result = await _otpService.VerifyOtpAsync(verifyOtpDto.Email, verifyOtpDto.Code);

            if (result == null)
            {
                _logger.LogWarning("OTP verification failed for {Email}.", verifyOtpDto.Email);
                return BadRequest("Invalid OTP code.");
            }
            _logger.LogInformation("OTP verified successfully for {Email}.", verifyOtpDto.Email);

            try
            {
                UserModel newUser = new UserModel
                {
                    Username = result.Username,
                    Email = result.Email,
                    PasswordHash = result.PasswordHash,
                    ProfilePicture = result.ProfilePicture
                };
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                try
                {
                    // Kullanıcı ayarları oluşturuluyor.
                    UserSettingsModel userSettings = new UserSettingsModel
                    {
                        UserId = newUser.UserId,
                        EntryDate = DateTime.UtcNow,
                        Notification = true,
                        Currency = "USD",
                        Language = "en",
                        Theme = "light"
                    };
                    await _context.UserSettings.AddAsync(userSettings);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create user settings for {Email}.", verifyOtpDto.Email);
                    return StatusCode(500, "An error occurred while creating user settings.");
                }

                try
                {
                    // Hoşgeldin e-postası gönderiliyor.
                    string welcomeEmailSubject = "Welcome to FinTrack!";
                    string welcomeEmailBody = string.Empty;

                    string welcomeEmailTemplatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Services", "EmailService", "EmailHtmlSchemes", "HelloScheme.html");
                    if (!System.IO.File.Exists(welcomeEmailTemplatePath))
                    {
                        _logger.LogError("Welcome email template not found at {Path}", welcomeEmailTemplatePath);
                        await _otpService.RemoveOtpAsync(verifyOtpDto.Email);
                        return StatusCode(500, "Welcome email template not found.");
                    }

                    using (StreamReader reader = new StreamReader(welcomeEmailTemplatePath))
                    {
                        welcomeEmailBody = await reader.ReadToEndAsync();
                    }

                    welcomeEmailBody = welcomeEmailBody.Replace("[UserName]", result.Username);
                    welcomeEmailBody = welcomeEmailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                    await _emailSender.SendEmailAsync(result.Email, welcomeEmailSubject, welcomeEmailBody);
                    _logger.LogInformation("Welcome email sent to {Email}.", result.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}.", result.Email);
                    return StatusCode(500, "Failed to send welcome email.");
                }

                return Ok(new { Message = "Registration successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user with email {Email}.", verifyOtpDto.Email);
                return StatusCode(500, "An error occurred while registering the user.");
            }
        }

        // Kullanıcı giriş işlemi yapılıyor.
        [HttpPost("user/login")]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Kullanıcı bulunamazsa veya şifre yanlışsa hata döndürülüyor.
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            // Kullanıcının giriş tarihi kayıt ediliyor.
            var userSettings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.UserId);

            if (userSettings != null)
            {
                userSettings.EntryDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var userRoles = new List<string> { "User" };
            Token token = TokenHandler.CreateToken(_configuration, user.UserId, user.Username, user.Email, userRoles); // JWT token oluşturuluyor.

            // Başarılı giriş sonrası kullanıcı bilgileri döndürülüyor.
            return Ok(new { user.UserId, user.Username, user.Email, user.ProfilePicture, AccessToken = token.AccessToken });
        }

        // TODO: Kullanıcı şifresini sıfırlama işlemi yapılacak
        // TODO: Kullanıcı çıkış yapabilecektir.

        [HttpPost("employee/login")]
        public async Task<IActionResult> EmployeeLogin([FromBody] LoginDto loginDto)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == loginDto.Email);


            if (employee == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, employee.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var employeeRoles = new List<string> { employee.EmployeeStatus.ToString() };
            Token token = TokenHandler.CreateToken(_configuration, employee.EmployeeId, employee.Name, employee.Email, employeeRoles);

            return Ok(new { employee.EmployeeId, employee.Name, employee.Email, employee.ProfilePictureUrl, AccessToken = token.AccessToken });
        }
    }
}

using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.AuthDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Otp;
using FinTrackWebApi.Models.User;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Identity;

namespace FinTrackWebApi.Services.Authentications
{
    public class UserAuthService : IUserAuthService
    {
        private readonly MyDataContext _context;
        private readonly ILogger<UserAuthService> _logger;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;

        public UserAuthService(
            MyDataContext context,
            ILogger<UserAuthService> logger, 
            UserManager<UserModel> userManager, 
            SignInManager<UserModel> signInManager, 
            IWebHostEnvironment webHostEnvironment,
            IOtpService otpService,
            IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _otpService = otpService;
            _emailSender = emailSender;
        }

        public async Task<(bool, string)> InitiateRegistration(UserInitiateRegistrationDto initiateDto)
        {
            string UserName = initiateDto.FirstName.Replace(" ", "").Trim() + "_" + initiateDto.LastName.Replace(" ", "").Trim();

            if (await _userManager.FindByEmailAsync(initiateDto.Email) != null)
            {
                _logger.LogWarning(
                    "Registration initiation failed: Email {Email} already exists in Identity.",
                    initiateDto.Email
                );
                return (false, "This email address is already registered.");
            }
            if (await _userManager.FindByNameAsync(UserName) != null)
            {
                _logger.LogWarning(
                    "Registration initiation failed: Username {Username} already exists in Identity.",
                    UserName
                );
                return (false, "This username is already taken.");
            }

            string otp = _otpService.GenerateOtp();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(5);
            string hashedOtp = BCrypt.Net.BCrypt.HashPassword(otp);

            bool stored = await _otpService.StoreOtpAsync(
                initiateDto.Email,
                hashedOtp,
                UserName,
                initiateDto.Password,
                initiateDto.ProfilePicture,
                expiryTime
            );

            if (!stored)
            {
                _logger.LogError("Failed to store OTP for {Email}.", initiateDto.Email);
                return (false, "An error occurred while initiating registration. Please try again.");
            }

            try
            {
                string emailSubject = "Email Verification Code For FinTrack new Membership";
                string emailBody = string.Empty;
                string emailTemplatePath = Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "Services",
                    "EmailService",
                    "EmailHtmlSchemes",
                    "CodeVerificationScheme.html"
                );

                if (!System.IO.File.Exists(emailTemplatePath))
                {
                    _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                    await _otpService.RemoveOtpAsync(initiateDto.Email);
                    return (false, "Email template not found.");
                }
                using (StreamReader reader = new StreamReader(emailTemplatePath))
                {
                    emailBody = await reader.ReadToEndAsync();
                }
                emailBody = emailBody.Replace("[UserName]", UserName);
                emailBody = emailBody.Replace("[VERIFICATION_CODE]", otp);
                emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                await _emailSender.SendEmailAsync(initiateDto.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send verification email to {Email}.",
                    initiateDto.Email
                );
                await _otpService.RemoveOtpAsync(initiateDto.Email);
                return (false, "Failed to send verification email. Please check the address and try again.");
            }

            _logger.LogInformation(
                "OTP sent to {Email} for registration initiation.",
                initiateDto.Email
            );
            return (true, "OTP has been sent to your email address. Please verify to complete registration.");
        }

        public void Login(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, string)> VerifyOtpAndRegister(VerifyOtpRequestDto verifyDto)
        {
            OtpVerificationModel? otpData = await _otpService.VerifyOtpAsync(
                verifyDto.Email,
                verifyDto.Code
            );

            if (otpData == null)
            {
                _logger.LogWarning(
                    "OTP verification failed for {Email} or OTP is invalid/expired.",
                    verifyDto.Email
                );
                return (false, "Invalid or expired OTP code.");
            }

            _logger.LogInformation("OTP verified successfully for {Email}.", verifyDto.Email);

            try
            {
                var newUser = new UserModel
                {
                    UserName = otpData.Username,
                    Email = otpData.Email,
                    EmailConfirmed = true,
                    ProfilePicture =
                        otpData.ProfilePicture
                        ?? "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740",
                    CreatedAtUtc = DateTime.UtcNow,
                };

                IdentityResult result = await _userManager.CreateAsync(
                    newUser,
                    otpData.TemporaryPlainPassword
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "User {Email} created successfully using UserManager. UserId: {UserId}",
                        newUser.Email,
                        newUser.Id
                    );

                    var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning(
                            "Failed to add user {Email} to 'User' role. Errors: {Errors}",
                            newUser.Email,
                            string.Join(", ", roleResult.Errors.Select(e => e.Description))
                        );
                    }

                    await _otpService.RemoveOtpAsync(verifyDto.Email);

                    try
                    {
                        var userAppSettings = new UserAppSettingsModel 
                        {
                            UserId = newUser.Id,
                            Appearance = AppearanceType.Light,
                            BaseCurrency = BaseCurrencyType.USD,
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        await _context.UserAppSettings.AddAsync(userAppSettings);

                        var userNotificationSettings = new UserNotificationSettingsModel
                        {
                            UserId = newUser.Id,
                            SpendingLimitWarning = true,
                            ExpectedBillReminder = true,
                            WeeklySpendingSummary = true,
                            NewFeaturesAndAnnouncements = true,
                            EnableDesktopNotifications = true,
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        await _context.UserNotificationSettings.AddAsync(userNotificationSettings);

                        var userMembership = new UserMembershipModel
                        {
                            UserId = newUser.Id,
                            MembershipPlanId = 1,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddYears(1),
                            Status = MembershipStatusType.Active,
                            AutoRenew = true,
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        await _context.UserMemberships.AddAsync(userMembership);

                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Initial settings and membership created for UserId: {UserId}", newUser.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create initial user settings/membership for UserId: {UserId}. The transaction will be rolled back.", newUser.Id);
                        await _userManager.DeleteAsync(newUser);
                        return (false, "An error occurred while setting up the user account. Please try again.");
                    }

                    try
                    {
                        string welcomeEmailSubject = "Welcome to FinTrack!";
                        string welcomeEmailBody = string.Empty;
                        string welcomeEmailTemplatePath = Path.Combine(
                            _webHostEnvironment.ContentRootPath,
                            "Services",
                            "EmailService",
                            "EmailHtmlSchemes",
                            "HelloScheme.html"
                        );

                        if (!System.IO.File.Exists(welcomeEmailTemplatePath))
                        {
                            _logger.LogError("Welcome email template not found at {Path}", welcomeEmailTemplatePath);
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(welcomeEmailTemplatePath))
                            {
                                welcomeEmailBody = await reader.ReadToEndAsync();
                            }
                            welcomeEmailBody = welcomeEmailBody.Replace("[UserName]", newUser.UserName);
                            welcomeEmailBody = welcomeEmailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));
                            await _emailSender.SendEmailAsync(newUser.Email, welcomeEmailSubject, welcomeEmailBody);
                            _logger.LogInformation("Welcome email sent to {Email}.", newUser.Email);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending welcome email to {Email}", newUser.Email);
                    }

                    return (true, $"User registration successful. You can now log in. UserId: {newUser.Id}");
                }
                else
                {
                    _logger.LogError(
                        "Failed to create Identity user for {Email}. Errors: {Errors}",
                        verifyDto.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                    return (true, $"User registration failed. Error: {result.Errors.Select(e => e.Description)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration for {Email}.", verifyDto.Email);
                if (otpData != null)
                    await _otpService.RemoveOtpAsync(otpData.Email);
                return (false, "An unexpected error occurred during registration.");
            }
        }
    }
}

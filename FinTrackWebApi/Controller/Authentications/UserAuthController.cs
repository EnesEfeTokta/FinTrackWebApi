using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.AuthDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using FinTrackWebApi.Security;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackWebApi.Controller.Authentications
{
    [Route("[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IOtpService _otpService;
        private readonly ILogger<UserAuthController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public UserAuthController(
            MyDataContext context,
            IConfiguration configuration,
            IOtpService otpService,
            IEmailSender emailService,
            ILogger<UserAuthController> logger,
            IWebHostEnvironment webHostEnvironment,
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager
        )
        {
            _context = context;
            _configuration = configuration;
            _otpService = otpService;
            _emailSender = emailService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("initiate-registration")]
        public async Task<IActionResult> UserInitiateRegistration(
            [FromBody] UserInitiateRegistrationDto initiateDto
        )
        {
            if (
                initiateDto == null
                || string.IsNullOrWhiteSpace(initiateDto.Email)
                || string.IsNullOrWhiteSpace(initiateDto.FirstName)
                || string.IsNullOrWhiteSpace(initiateDto.LastName)
                || string.IsNullOrWhiteSpace(initiateDto.Password)
            )
            {
                return BadRequest(new { Message = "Email, Username, and Password are required." });
            }

            string UserName = initiateDto.FirstName.Trim() + "_" + initiateDto.LastName.Trim();

            if (await _userManager.FindByEmailAsync(initiateDto.Email) != null)
            {
                _logger.LogWarning(
                    "Registration initiation failed: Email {Email} already exists in Identity.",
                    initiateDto.Email
                );
                return BadRequest(new { Message = "This email address is already registered." });
            }
            if (await _userManager.FindByNameAsync(UserName) != null)
            {
                _logger.LogWarning(
                    "Registration initiation failed: Username {Username} already exists in Identity.",
                    UserName
                );
                return BadRequest(new { Message = "This username is already taken." });
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
                return StatusCode(
                    500,
                    new
                    {
                        Message = "An error occurred while initiating registration. Please try again.",
                    }
                );
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
                    return StatusCode(500, new { Message = "Email template not found." });
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
                return StatusCode(
                    500,
                    new
                    {
                        Message = "Failed to send verification email. Please check the address and try again.",
                    }
                );
            }

            _logger.LogInformation(
                "OTP sent to {Email} for registration initiation.",
                initiateDto.Email
            );
            return Ok(
                new
                {
                    Message = "OTP has been sent to your email address. Please verify to complete registration.",
                }
            );
        }

        [HttpPost("verify-otp-and-register")]
        public async Task<IActionResult> UserVerifyOtpAndRegister(
            [FromBody] VerifyOtpRequestDto verifyDto
        )
        {
            if (
                verifyDto == null
                || string.IsNullOrWhiteSpace(verifyDto.Email)
                || string.IsNullOrWhiteSpace(verifyDto.Code)
            )
            {
                return BadRequest(new { Message = "Email and OTP code are required." });
            }

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
                return BadRequest(new { Message = "Invalid or expired OTP code." });
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

                    #region User Settings Creation
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
                        await _context.SaveChangesAsync();
                        _logger.LogInformation(
                            "UserSettings created for UserId: {UserId}",
                            newUser.Id
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to create user app settings for UserId: {UserId}",
                            newUser.Id
                        );
                    }

                    try
                    {
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
                        await _context.UserNotificationSettings.AddAsync(
                            userNotificationSettings
                        );
                        await _context.SaveChangesAsync();
                        _logger.LogInformation(
                            "UserNotificationSettings created for UserId: {UserId}",
                            newUser.Id
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to create user notification settings for UserId: {UserId}",
                            newUser.Id
                        );
                    }
                    #endregion

                    #region User Memebership Creation
                    try
                    {
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
                        _logger.LogInformation(
                            "UserMembership created for UserId: {UserId}",
                            newUser.Id
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to create user membership for UserId: {UserId}",
                            newUser.Id
                        );
                    }
                    #endregion

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
                            _logger.LogError(
                                "Welcome email template not found at {Path}",
                                welcomeEmailTemplatePath
                            );
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(welcomeEmailTemplatePath))
                            {
                                welcomeEmailBody = await reader.ReadToEndAsync();
                            }
                            welcomeEmailBody = welcomeEmailBody.Replace(
                                "[UserName]",
                                newUser.UserName
                            );
                            welcomeEmailBody = welcomeEmailBody.Replace(
                                "[YEAR]",
                                DateTime.UtcNow.ToString("yyyy")
                            );
                            await _emailSender.SendEmailAsync(
                                newUser.Email,
                                welcomeEmailSubject,
                                welcomeEmailBody
                            );
                            _logger.LogInformation("Welcome email sent to {Email}.", newUser.Email);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error sending welcome email to {Email}",
                            newUser.Email
                        );
                    }

                    return Ok(
                        new
                        {
                            Message = "User registration successful. You can now log in.",
                            UserId = newUser.Id,
                        }
                    );
                }
                else
                {
                    _logger.LogError(
                        "Failed to create Identity user for {Email}. Errors: {Errors}",
                        verifyDto.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                    return BadRequest(
                        new
                        {
                            Message = "User registration failed.",
                            Errors = result.Errors.Select(e => e.Description),
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred during user registration for {Email}.",
                    verifyDto.Email
                );
                if (otpData != null)
                    await _otpService.RemoveOtpAsync(otpData.Email);
                return StatusCode(
                    500,
                    new { Message = "An unexpected error occurred during registration." }
                );
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto loginDto)
        {
            if (
                loginDto == null
                || string.IsNullOrWhiteSpace(loginDto.Email)
                || string.IsNullOrWhiteSpace(loginDto.Password)
            )
            {
                return BadRequest(new { Message = "Email and Password are required." });
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning(
                    "Login attempt for email {Email} failed: User not found.",
                    loginDto.Email
                );
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                loginDto.Password,
                lockoutOnFailure: true
            );
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(
                        "User account locked out for email {Email}.",
                        loginDto.Email
                    );
                    return Unauthorized(
                        new
                        {
                            Message = $"Account locked out. Please try again later. (Until: {user.LockoutEnd?.ToLocalTime()})",
                            IsLockedOut = true,
                            LockoutEndDateUtc = user.LockoutEnd,
                        }
                    );
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning(
                        "Login not allowed for email {Email} (e.g., email not confirmed, if configured).",
                        loginDto.Email
                    );
                    return Unauthorized(
                        new
                        {
                            Message = "Login not allowed. Please confirm your email or contact support.",
                        }
                    );
                }
                _logger.LogWarning(
                    "Login attempt for email {Email} failed: Invalid password. AccessFailedCount: {AccessFailedCount}",
                    loginDto.Email,
                    user.AccessFailedCount
                );
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
            var userRoles = await _userManager.GetRolesAsync(user);

            Token generatedToken = TokenHandler.CreateToken(
                _configuration,
                user.Id,
                user.UserName ?? "",
                user.Email ?? "",
                userRoles
            );

            //var userSettings = await _context.UserSettings.FirstOrDefaultAsync(s =>
            //    s.UserId == user.Id
            //);
            //if (userSettings != null)
            //{
            //    userSettings.EntryDate = DateTime.UtcNow;
            //    await _context.SaveChangesAsync();
            //}

            return Ok(
                new
                {
                    UserId = user.Id,
                    user.UserName,
                    user.Email,
                    user.ProfilePicture,
                    generatedToken.AccessToken,
                    generatedToken.RefreshToken,
                    Roles = userRoles,
                }
            );
        }
    }
}
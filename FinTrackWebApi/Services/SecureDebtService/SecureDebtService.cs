using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using FinTrackWebApi.Models.Debt;
using FinTrackWebApi.Models.User;
using FinTrackWebApi.Services.EmailService;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace FinTrackWebApi.Services.SecureDebtService
{
    internal class NotificationTemplate
    {
        public string MessageHead { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class SecureDebtService : ISecureDebtService
    {
        private readonly MyDataContext _context;
        private readonly UserManager<UserModel> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SecureDebtService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly string _notificationTemplatePath;
        private readonly string _emailTemplatePath;

        public SecureDebtService(
            MyDataContext context,
            UserManager<UserModel> userManager,
            IEmailSender emailSender,
            ILogger<SecureDebtService> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;

            _notificationTemplatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Services",
                "NotificationTemplates",
                "MessagesSchemes.json"
            );

            _emailTemplatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Services",
                "EmailService",
                "EmailHtmlSchemes",
                "BorrowerInformationScheme.html"
            );
        }

        public async Task<CreateDebtOfferResult> CreateDebtOfferAsync(
            UserModel lender,
            UserModel borrower,
            decimal amount,
            BaseCurrencyType currency,
            DateTime dueDate,
            string? description
        )
        {
            var debt = new DebtModel
            {
                LenderId = lender.Id,
                BorrowerId = borrower.Id,
                Amount = amount,
                Currency = currency,
                DueDateUtc = DateTime.SpecifyKind(dueDate, DateTimeKind.Unspecified).ToUniversalTime(),
                Description = description ?? "No description available.",
                CreateAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Status = DebtStatusType.PendingBorrowerAcceptance,
            };

            try
            {
                await _context.Debts.AddAsync(debt);

                var notification = await CreateInAppNotificationAsync(debt, lender, borrower);
                if (notification != null)
                {
                    await _context.Notifications.AddAsync(notification);
                }
                else
                {
                    _logger.LogWarning("In-app notification could not be created. Aborting debt creation.");
                    return new CreateDebtOfferResult
                    {
                        Success = false,
                        Message = "Teklif oluşturulamadı (bildirim şablonu hatası)."
                    };
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Debt (ID: {DebtId}) and its notification were successfully saved to the database.", debt.Id);

                bool emailSent = await SendDebtOfferEmailAsync(debt, lender, borrower);
                if (!emailSent)
                {
                    return new CreateDebtOfferResult
                    {
                        Success = true,
                        Message = "Borç teklifi oluşturuldu ancak bilgilendirme e-postası gönderilemedi.",
                        DebtId = debt.Id
                    };
                }

                return new CreateDebtOfferResult
                {
                    Success = true,
                    Message = "Borç teklifi başarıyla oluşturuldu ve borçluya iletildi.",
                    DebtId = debt.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating a debt offer for LenderId: {LenderId} and BorrowerId: {BorrowerId}", lender.Id, borrower.Id);
                return new CreateDebtOfferResult
                {
                    Success = false,
                    Message = "Teklif oluşturulurken beklenmedik bir hata oluştu."
                };
            }
        }

        private async Task<bool> SendDebtOfferEmailAsync(DebtModel debt, UserModel lender, UserModel borrower)
        {
            try
            {
                if (!File.Exists(_emailTemplatePath))
                {
                    _logger.LogError("Email template not found at {Path}", _emailTemplatePath);
                    return false;
                }

                string emailBody = await File.ReadAllTextAsync(_emailTemplatePath);

                emailBody = emailBody.Replace("[BORROWER_NAME]", borrower.UserName)
                                     .Replace("[LENDER_NAME]", lender.UserName)
                                     .Replace("[DETAIL_LENDER_NAME]", lender.UserName)
                                     .Replace("[DETAIL_DEBT_AMOUNT]", debt.Amount.ToString("F2"))
                                     .Replace("[DETAIL_DEBT_CURRENCY]", debt.Currency.ToString())
                                     .Replace("[DETAIL_DEBT_DUE_DATE]", debt.DueDateUtc.ToString("yyyy-MM-dd HH:mm:ss UTC"))
                                     .Replace("[DETAIL_DEBT_DESCRIPTION]", debt.Description)
                                     .Replace("[YEAR]", DateTime.UtcNow.Year.ToString());

                await _emailSender.SendEmailAsync(
                    borrower.Email ?? throw new ArgumentNullException(nameof(borrower.Email), "Borrower's email is required."),
                    "Yeni Güvenli Borç Teklifi",
                    emailBody
                );

                _logger.LogInformation("Debt offer email successfully sent to {BorrowerEmail} for DebtId: {DebtId}.", borrower.Email, debt.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send debt offer email for DebtId: {DebtId}", debt.Id);
                return false;
            }
        }

        private async Task<NotificationModel?> CreateInAppNotificationAsync(DebtModel debt, UserModel lender, UserModel borrower)
        {
            try
            {
                if (!File.Exists(_notificationTemplatePath))
                {
                    _logger.LogError("Notification template not found at {Path}", _notificationTemplatePath);
                    return null;
                }

                string jsonContent = await File.ReadAllTextAsync(_notificationTemplatePath);

                var template = JsonSerializer.Deserialize<NotificationTemplate>(jsonContent);

                if (template == null)
                {
                    _logger.LogError("Failed to deserialize notification template from {Path}", _notificationTemplatePath);
                    return null;
                }

                if (!Enum.TryParse(template.Type, true, out NotificationType notificationType))
                {
                    _logger.LogWarning("Invalid notification type '{Type}' in template. Defaulting to 'Info'.", template.Type);
                    notificationType = NotificationType.Info;
                }

                string messageBody = template.MessageBody
                    .Replace("[LENDER_USERNAME]", lender.UserName)
                    .Replace("[CREATE_DATE]", debt.CreateAtUtc.ToString("dd.MM.yyyy"))
                    .Replace("[DUE_DATE]", debt.DueDateUtc.ToString("dd.MM.yyyy"))
                    .Replace("[AMOUNT]", $"{debt.Amount:F2} {debt.Currency}");

                var notification = new NotificationModel
                {
                    UserId = borrower.Id,
                    MessageHead = template.MessageHead,
                    MessageBody = messageBody,
                    Type = notificationType,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsRead = false
                };

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create in-app notification model for DebtId: {DebtId}", debt.Id);
                return null;
            }
        }
    }
}
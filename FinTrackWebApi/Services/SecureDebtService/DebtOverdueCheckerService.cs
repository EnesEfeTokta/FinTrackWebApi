using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Debt;
using FinTrackWebApi.Services.EmailService;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Services.SecureDebtService
{
    public class DebtOverdueCheckerService : BackgroundService
    {
        private readonly ILogger<DebtOverdueCheckerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public DebtOverdueCheckerService(
            ILogger<DebtOverdueCheckerService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Debt Overdue Checker Service is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndMarkOverdueDebtsAsync(stoppingToken);
                await Task.Delay(_period, stoppingToken);
            }
            _logger.LogInformation("Debt Overdue Checker Service is stopping.");
        }

        private async Task CheckAndMarkOverdueDebtsAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDataContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                try
                {
                    var utcNow = DateTime.UtcNow.Date;
                    var overdueDebts = await dbContext.Debts
                            .Include(d => d.Lender)
                            .Include(d => d.Borrower)
                            .Where(d => d.Status == DebtStatusType.Active && d.DueDateUtc.Date < utcNow)
                            .ToListAsync(stoppingToken);

                    if (overdueDebts.Any())
                    {
                        foreach (var debt in overdueDebts)
                        {
                            debt.Status = DebtStatusType.Defaulted;
                            debt.UpdatedAtUtc = DateTime.UtcNow;
                            await SendOverdueEmailAsync(debt, emailSender, webHostEnvironment);
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in DebtOverdueCheckerService: {ErrorMessage}", ex.Message);
                }
            }
        }

        private async Task SendOverdueEmailAsync(DebtModel debt, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            if (string.IsNullOrEmpty(debt.Lender?.Email))
            {
                _logger.LogWarning("Cannot send overdue email for DebtId {DebtId} because lender email is missing.", debt.Id);
                return;
            }

            string emailSubject = "Your Payment is Overdue";
            string emailBody;

            string emailTemplatePath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Services",
                "EmailService",
                "EmailHtmlSchemes",
                "DebtLastPaymentDateApproximationScheme.html"
            );

            if (!File.Exists(emailTemplatePath))
            {
                _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                return;
            }

            emailBody = await File.ReadAllTextAsync(emailTemplatePath);

            emailBody = emailBody.Replace("[BORROWER_NAME]", debt.Borrower?.UserName ?? "N/A");
            emailBody = emailBody.Replace("[LENDER_NAME]", debt.Lender?.UserName ?? "N/A");
            emailBody = emailBody.Replace("[AGREEMENT_ID]", debt.Id.ToString());
            emailBody = emailBody.Replace("[DETAIL_LENDER_NAME]", debt.Lender?.UserName ?? "N/A");
            emailBody = emailBody.Replace("[DETAIL_BORROWER_NAME]", debt.Borrower?.UserName ?? "N/A");
            emailBody = emailBody.Replace("[DETAIL_AGREEMENT_ID]", debt.Id.ToString());
            emailBody = emailBody.Replace("[DETAIL_DEBT_AMOUNT]", debt.Amount.ToString("F2"));
            emailBody = emailBody.Replace("[ORIGINAL_DUE_DATE]", debt.DueDateUtc.ToString("dd-MM-yyyy"));
            emailBody = emailBody.Replace("[DETAIL_DEBT_DESCRIPTION]", debt.Description ?? "No description provided.");
            emailBody = emailBody.Replace("[OVERDUE_AMOUNT]", debt.Amount.ToString("F2"));
            emailBody = emailBody.Replace("[CURRENCY]", debt.Currency.ToString());
            emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

            await emailSender.SendEmailAsync(
                debt.Lender.Email,
                emailSubject,
                emailBody
            );

            _logger.LogInformation("Sent overdue notification email for DebtId: {DebtId} to {Email}", debt.Id, debt.Lender.Email);
        }
    }
}
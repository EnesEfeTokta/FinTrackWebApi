using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Debt;
using FinTrackWebApi.Models.User;
using FinTrackWebApi.Services.EmailService;
using Microsoft.AspNetCore.Identity;

namespace FinTrackWebApi.Services.SecureDebtService
{
    public class SecureDebtService : ISecureDebtService
    {
        private readonly MyDataContext _context;
        private readonly UserManager<UserModel> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SecureDebtService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

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
        }

        // Borç teklifi oluşturma metodu.
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
                DueDateUtc = DateTime
                    .SpecifyKind(dueDate, DateTimeKind.Unspecified)
                    .ToUniversalTime(),
                Description = description ?? "No description available.",
                CreateAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Status = DebtStatusType.PendingBorrowerAcceptance,
            };

            try
            {
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - debt.LenderId: {LenderId}, debt.BorrowerId: {BorrowerId}, debt.CurrencyId: {CurrencyId}",
                    debt.LenderId,
                    debt.BorrowerId,
                    debt.Currency
                );
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - lender object from UserManager - Id: {LenderId}, UserName: {LenderUserName}",
                    lender.Id,
                    lender.UserName
                );
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - borrower object from UserManager - Id: {BorrowerId}, UserName: {BorrowerUserName}",
                    borrower.Id,
                    borrower.UserName
                );

                await _context.Debts.AddAsync(debt);

                _logger.LogInformation(
                    "STAGE 2: After AddAsync, Before SaveChanges - debt.LenderId: {LenderId}, debt.BorrowerId: {BorrowerId}",
                    debt.LenderId,
                    debt.BorrowerId
                );

                var debtEntry = _context
                    .ChangeTracker.Entries<DebtModel>()
                    .FirstOrDefault(e => e.Entity == debt);
                if (debtEntry != null)
                {
                    _logger.LogInformation(
                        "DebtEntry State: {State}, Current LenderId: {LenderId}, Current BorrowerId: {BorrowerId}",
                        debtEntry.State,
                        debtEntry.Property(d => d.LenderId).CurrentValue,
                        debtEntry.Property(d => d.BorrowerId).CurrentValue
                    );

                    var lenderNavEntry = debtEntry.Reference(d => d.Lender).TargetEntry;
                    if (lenderNavEntry != null)
                    {
                        _logger.LogInformation(
                            "Debt's Lender Navigation Property points to UserModel with Id: {NavLenderId}, State: {NavLenderState}",
                            lenderNavEntry.Property("Id").CurrentValue,
                            lenderNavEntry.State
                        );
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Debt's Lender Navigation Property is NULL or not tracked."
                        );
                    }

                    var borrowerNavEntry = debtEntry.Reference(d => d.Borrower).TargetEntry;
                    if (borrowerNavEntry != null)
                    {
                        _logger.LogInformation(
                            "Debt's Borrower Navigation Property points to UserModel with Id: {NavBorrowerId}, State: {NavBorrowerState}",
                            borrowerNavEntry.Property("Id").CurrentValue,
                            borrowerNavEntry.State
                        );
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Debt's Borrower Navigation Property is NULL or not tracked."
                        );
                    }
                }
                else
                {
                    _logger.LogWarning("Debt entity not found in ChangeTracker after AddAsync!");
                }

                var trackedUsers = _context.ChangeTracker.Entries<UserModel>().ToList();
                if (trackedUsers.Any())
                {
                    _logger.LogInformation(
                        "--- Tracked UserModels BEFORE SaveChanges ({Count}) ---",
                        trackedUsers.Count
                    );
                    foreach (var userEntry in trackedUsers)
                    {
                        var userEntity = userEntry.Entity;
                        _logger.LogInformation(
                            "Tracked UserModel - Id: {UserId}, UserName: {UserName}, Email: {Email}, State: {State}",
                            userEntity.Id,
                            userEntity.UserName,
                            userEntity.Email,
                            userEntry.State
                        );
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "No UserModels are currently tracked by the DbContext BEFORE SaveChanges."
                    );
                }
                _logger.LogInformation("--- End of ChangeTracker State BEFORE SaveChanges ---");

                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "STAGE 3: After SaveChanges - debt.LenderId: {LenderId}, debt.BorrowerId: {BorrowerId}",
                    debt.LenderId,
                    debt.BorrowerId
                );

                try
                {
                    string subject = "Request New Secured Debt";
                    string emailBody = string.Empty;

                    string emailTemplatePath = Path.Combine(
                        _webHostEnvironment.ContentRootPath,
                        "Services",
                        "EmailService",
                        "EmailHtmlSchemes",
                        "BorrowerInformationScheme.html"
                    );
                    if (!File.Exists(emailTemplatePath))
                    {
                        _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                        return new CreateDebtOfferResult
                        {
                            Success = false,
                            Message = "E-posta şablonu bulunamadı.",
                        };
                    }

                    using (StreamReader reader = new StreamReader(emailTemplatePath))
                    {
                        emailBody = await reader.ReadToEndAsync();
                    }

                    emailBody = emailBody.Replace("[BORROWER_NAME]", borrower.UserName);
                    emailBody = emailBody.Replace("[LENDER_NAME]", lender.UserName);
                    emailBody = emailBody.Replace("[DETAIL_LENDER_NAME]", lender.UserName);
                    emailBody = emailBody.Replace("[DETAIL_DEBT_AMOUNT]", debt.Amount.ToString());
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_CURRENCY]",
                        debt.Currency.ToString() ?? "Unknown Currency"
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_DUE_DATE]",
                        debt.DueDateUtc.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                    emailBody = emailBody.Replace("[DETAIL_DEBT_DESCRIPTION]", debt.Description);
                    emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                    await _emailSender.SendEmailAsync(
                        borrower.Email
                            ?? throw new ArgumentException("We need the borrower's email address."),
                        subject,
                        emailBody
                    );
                    _logger.LogInformation(
                        "Debt offer (ID: {DebtId}) was successfully created and a notification was sent to {BorrowerEmail}.",
                        debt.Id,
                        borrower.Email
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(
                        emailEx,
                        "An error occurred while sending the debt offer email. Debt offer ID: {DebtId}",
                        debt.Id
                    );
                    return new CreateDebtOfferResult
                    {
                        Success = false,
                        Message = "The quote has been created, but the email notification failed.",
                    };
                }

                // TODO: Eğer borç veren kullanıcıya da bildirim göndermek gerekli. Bunun için Bildirim servisi oluşturulmalı.
                return new CreateDebtOfferResult
                {
                    Success = true,
                    Message =
                        "The debt offer was successfully created and communicated to the debtor.",
                    CreatedDebt = debt,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating a debt offer.");
                return new CreateDebtOfferResult
                {
                    Success = false,
                    Message = "Offer could not be created (general error).",
                };
            }
        }
    }
}

using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            IWebHostEnvironment webHostEnvironment)
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
            string lenderId,
            string borrowerEmail,
            decimal amount,
            CurrencyModel currency,
            DateTime dueDate,
            string? description)
        {
            var lender = await _userManager.FindByIdAsync(lenderId);
            if (lender == null)
            {
                return new CreateDebtOfferResult { Success = false, Message = "Borç veren kullanıcı bulunamadı." };
            }

            var borrower = await _userManager.FindByEmailAsync(borrowerEmail);
            if (borrower == null)
            {
                _logger.LogWarning("Borç teklifi için borç alacak kullanıcı bulunamadı: {BorrowerEmail}", borrowerEmail);
                return new CreateDebtOfferResult { Success = false, Message = $"'{borrowerEmail}' e-posta adresine sahip kullanıcı bulunamadı." };
            }

            if (lender.Id == borrower.Id)
            {
                return new CreateDebtOfferResult { Success = false, Message = "Kendinize borç teklif edemezsiniz." };
            }

            var debt = new DebtModel
            {
                LenderId = lender.Id,
                BorrowerId = borrower.Id,
                Amount = amount,
                CurrencyId = currency.CurrencyId,
                DueDateUtc = DateTime.SpecifyKind(dueDate, DateTimeKind.Unspecified).ToUniversalTime(),
                Description = description ?? "Açıklama yok.",
                CreateAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Status = DebtStatus.PendingBorrowerAcceptance
            };

            try
            {
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - debt.LenderId: {LenderId}, debt.BorrowerId: {BorrowerId}, debt.CurrencyId: {CurrencyId}",
                    debt.LenderId, debt.BorrowerId, debt.CurrencyId);
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - lender object from UserManager - Id: {LenderId}, UserName: {LenderUserName}",
                    lender.Id, lender.UserName); // lender nesnesinin ID'si nedir?
                _logger.LogInformation(
                    "STAGE 1: Before AddAsync - borrower object from UserManager - Id: {BorrowerId}, UserName: {BorrowerUserName}",
                    borrower.Id, borrower.UserName); // borrower nesnesinin ID'si nedir?

                await _context.Debts.AddAsync(debt);

                // --- STAGE 2: After AddAsync, BEFORE SaveChanges ---
                _logger.LogInformation(
                    "STAGE 2: After AddAsync, Before SaveChanges - debt.LenderId: {LenderId}, debt.BorrowerId: {BorrowerId}",
                    debt.LenderId, debt.BorrowerId);

                // Change Tracker'ı detaylı incele:
                _logger.LogInformation("--- ChangeTracker State BEFORE SaveChanges ---");

                // DebtModel girdisini bul
                var debtEntry = _context.ChangeTracker.Entries<DebtModel>().FirstOrDefault(e => e.Entity == debt);
                if (debtEntry != null)
                {
                    _logger.LogInformation(
                        "DebtEntry State: {State}, Current LenderId: {LenderId}, Current BorrowerId: {BorrowerId}",
                        debtEntry.State,
                        debtEntry.Property(d => d.LenderId).CurrentValue,
                        debtEntry.Property(d => d.BorrowerId).CurrentValue);

                    // Lender navigation property'sinin işaret ettiği UserModel'in ID'sine bak
                    var lenderNavEntry = debtEntry.Reference(d => d.Lender).TargetEntry;
                    if (lenderNavEntry != null)
                    {
                        _logger.LogInformation(
                            "Debt's Lender Navigation Property points to UserModel with Id: {NavLenderId}, State: {NavLenderState}",
                            lenderNavEntry.Property("Id").CurrentValue, // "Id" property adının doğru olduğundan emin olun
                            lenderNavEntry.State);
                    }
                    else
                    {
                        _logger.LogInformation("Debt's Lender Navigation Property is NULL or not tracked.");
                    }

                    // Borrower navigation property'sinin işaret ettiği UserModel'in ID'sine bak
                    var borrowerNavEntry = debtEntry.Reference(d => d.Borrower).TargetEntry;
                    if (borrowerNavEntry != null)
                    {
                        _logger.LogInformation(
                            "Debt's Borrower Navigation Property points to UserModel with Id: {NavBorrowerId}, State: {NavBorrowerState}",
                            borrowerNavEntry.Property("Id").CurrentValue, // "Id" property adının doğru olduğundan emin olun
                            borrowerNavEntry.State);
                    }
                    else
                    {
                        _logger.LogInformation("Debt's Borrower Navigation Property is NULL or not tracked.");
                    }
                }
                else
                {
                    _logger.LogWarning("Debt entity not found in ChangeTracker after AddAsync!");
                }

                // _context tarafından takip edilen TÜM UserModel örneklerini logla
                var trackedUsers = _context.ChangeTracker.Entries<UserModel>().ToList();
                if (trackedUsers.Any())
                {
                    _logger.LogInformation("--- Tracked UserModels BEFORE SaveChanges ({Count}) ---", trackedUsers.Count);
                    foreach (var userEntry in trackedUsers)
                    {
                        var userEntity = userEntry.Entity;
                        _logger.LogInformation(
                            "Tracked UserModel - Id: {UserId}, UserName: {UserName}, Email: {Email}, State: {State}",
                            userEntity.Id, userEntity.UserName, userEntity.Email, userEntry.State);
                        // Özellikle negatif ID'li kullanıcılar var mı ve state'leri ne?
                    }
                }
                else
                {
                    _logger.LogInformation("No UserModels are currently tracked by the DbContext BEFORE SaveChanges.");
                }
                _logger.LogInformation("--- End of ChangeTracker State BEFORE SaveChanges ---");


                await _context.SaveChangesAsync();

                // E-Posta bildirimini gönderme.
                string subject = "Request New Secured Debt";
                string emailBody = string.Empty;

                string emailTemplatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Services", "EmailService", "EmailHtmlSchemes", "BorrowerInformationScheme.html");
                if (!File.Exists(emailTemplatePath))
                {
                    _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                    return new CreateDebtOfferResult { Success = false, Message = "E-posta şablonu bulunamadı." };
                }

                using (StreamReader reader = new StreamReader(emailTemplatePath))
                {
                    emailBody = await reader.ReadToEndAsync();
                }

                emailBody = emailBody.Replace("[BORROWER_NAME]", borrower.UserName);
                emailBody = emailBody.Replace("[LENDER_NAME]", lender.UserName);
                emailBody = emailBody.Replace("[DETAIL_LENDER_NAME]", lender.UserName);
                emailBody = emailBody.Replace("[DETAIL_DEBT_AMOUNT]", debt.Amount.ToString());
                emailBody = emailBody.Replace("[DETAIL_DEBT_CURRENCY]", debt.CurrencyModel?.Name ?? "Bilinmiyor");
                emailBody = emailBody.Replace("[DETAIL_DEBT_DUE_DATE]", debt.DueDateUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                emailBody = emailBody.Replace("[DETAIL_DEBT_DESCRIPTION]", debt.Description);
                emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                await _emailSender.SendEmailAsync(borrower.Email ?? throw new ArgumentException("We need the borrower's email address."), subject, emailBody);
                _logger.LogInformation("Borç teklifi (ID: {DebtId}) başarıyla oluşturuldu ve {BorrowerEmail} adresine bildirim gönderildi.", debt.DebtId, borrower.Email);

                // TODO: Eğer borç veren kullanıcıya da bildirim göndermek gerekli. Bunun için Bildirim servisi oluşturulmalı.
                return new CreateDebtOfferResult { Success = true, Message = "Borç teklifi başarıyla oluşturuldu ve borç alacak kişiye bildirildi.", CreatedDebt = debt };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Borç teklifi oluşturulurken veritabanı hatası.");
                return new CreateDebtOfferResult { Success = false, Message = "Teklif oluşturulamadı (veritabanı)." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Borç teklifi oluşturulurken beklenmedik hata.");
                return new CreateDebtOfferResult { Success = false, Message = "Teklif oluşturulamadı (genel hata)." };
            }
        }

        // Borç bilgilerini alma metotları.
        public async Task<DebtModel?> GetDebtByIdAsync(int debtId)
        {
            try
            {
                return await _context.Debts
                    .Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .FirstOrDefaultAsync(d => d.DebtId == debtId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Borç bilgisi alınırken hata oluştu.");
                return null;
            }
        }

        // Kullanıcıya ait borçları alma metodu.
        public async Task<List<DebtModel>> GetDebtsByIdAsync(int Id)
        {
            try
            {
                return await _context.Debts
                    .Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.LenderId == Id || d.BorrowerId == Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı borçları alınırken hata oluştu.");
                return new List<DebtModel>();
            }
        }
    }
}
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
            string lenderUserId,
            string borrowerEmail,
            decimal amount,
            CurrencyModel currency,
            DateTime dueDate,
            string? description)
        {
            var lender = await _userManager.FindByIdAsync(lenderUserId);
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

            if (lender.UserId == borrower.UserId)
            {
                return new CreateDebtOfferResult { Success = false, Message = "Kendinize borç teklif edemezsiniz." };
            }

            var debt = new DebtModel
            {
                LenderId = lender.UserId,
                BorrowerId = borrower.UserId,
                Amount = amount,
                CurrencyModel = currency,
                DueDateUtc = DateTime.SpecifyKind(dueDate, DateTimeKind.Unspecified).ToUniversalTime(),
                Description = description ?? "Açıklama yok.",
                CreateAtUtc = DateTime.UtcNow,
                Status = DebtStatus.PendingBorrowerAcceptance
            };

            try
            {
                await _context.Debts.AddAsync(debt);
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

                emailBody.Replace("[BORROWER_NAME]", borrower.Username);
                emailBody = emailBody.Replace("[LENDER_NAME]", lender.Username);
                emailBody = emailBody.Replace("[DETAIL_LENDER_NAME]", lender.Username);
                emailBody = emailBody.Replace("[DETAIL_DEBT_AMOUNT]", debt.Amount.ToString());
                emailBody = emailBody.Replace("[DETAIL_DEBT_CURRENCY]", debt.CurrencyModel?.Name ?? "Bilinmiyor");
                emailBody = emailBody.Replace("[DETAIL_DEBT_DUE_DATE]", debt.DueDateUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                emailBody = emailBody.Replace("[DETAIL_DEBT_DESCRIPTION]", debt.Description);
                emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                await _emailSender.SendEmailAsync(borrower.Email, subject, emailBody);
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
        public async Task<List<DebtModel>> GetDebtsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Debts
                    .Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.LenderId == userId || d.BorrowerId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı borçları alınırken hata oluştu.");
                return new List<DebtModel>();
            }
        }

        // Borç teklifini kabul etme metodu.
        public async Task<bool> AcceptDebtOfferAsync(int debtId, string borrowerUserId)
        {
            var debt = await _context.Debts.FindAsync(debtId);
            if (debt == null)
            {
                _logger.LogWarning("Borç teklifi bulunamadı: {DebtId}", debtId);
                return false;
            }
            if (debt.BorrowerId.ToString() != borrowerUserId)
            {
                _logger.LogWarning("Borç teklifi kabul edilemedi. Kullanıcı yetkisi uyuşmuyor: {BorrowerUserId}", borrowerUserId);
                return false;
            }

            // TODO: Borcun onaylanması için borç alanın videosu alınması ve doğrulanması gerekiyor.

            debt.Status = DebtStatus.PendingOperatorApproval;
            try
            {
                _context.Debts.Update(debt);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Borç teklifi (ID: {DebtId}) başarıyla kabul edildi. Operatör onayı bekleniliyor.", debt.DebtId);

                // TODO: Borç veren kullanıcıya bildirim gönderilebilir. Bunun için Bildirim servisi oluşturulmalı.
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Borç teklifi kabul edilirken hata oluştu.");
                return false;
            }
        }
    }
}
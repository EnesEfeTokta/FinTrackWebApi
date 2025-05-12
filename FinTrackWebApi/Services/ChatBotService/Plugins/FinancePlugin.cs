using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models; // CategoryModel'deki enum için (CategoryType)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinTrackWebApi.Services.ChatBotService.Plugins
{
    public class FinancePlugin
    {
        private readonly MyDataContext _context; // IDbContextFactory yerine doğrudan MyDataContext
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FinancePlugin> _logger;

        public FinancePlugin(
            MyDataContext context, // Doğrudan MyDataContext enjekte ediliyor
            IHttpContextAccessor httpContextAccessor,
            ILogger<FinancePlugin> logger)
        {
            _context = context; // Atama yapılıyor
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("FinancePlugin: Kullanıcı kimliği (UserId) token'dan alınamadı, boş veya geçersiz formatta. Claim Value: {ClaimValue}", userIdClaim);
                throw new UnauthorizedAccessException("Geçersiz veya eksik kullanıcı kimliği. Lütfen tekrar giriş yapın.");
            }
            return userId;
        }

        [KernelFunction, Description("Kullanıcının tüm gelir ve gider işlemlerini listeler. LLM GÖREVİ: Bu fonksiyonu çağır ve dönen listeyi kullanıcıya anlaşılır bir şekilde özetle.")]
        public async Task<List<TransactionDto>> GetAllTransactionsAsync()
        {
            // Artık _contextFactory.CreateDbContext() yok, doğrudan _context kullanılıyor.
            // MyDataContext Scoped olarak kaydedildiği için, FinancePlugin de Scoped veya Transient
            // ise bu HTTP isteği boyunca aynı DbContext örneğini kullanır (veya Transient ise her seferinde yeni bir scope'tan alır).
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("FinancePlugin: GetAllTransactionsAsync çağrıldı (direct DbContext). UserId: {UserId}", userId);

                var transactions = await _context.Transactions
                    .AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDateUtc)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CategoryType = t.Category.Type, // DTO'daki alan string ise
                        UserId = t.UserId,
                        AccountId = t.AccountId,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        TransactionDateUtc = t.TransactionDateUtc,
                        Description = t.Description
                    })
                    .ToListAsync();

                if (!transactions.Any())
                {
                    _logger.LogInformation("FinancePlugin: GetAllTransactionsAsync - UserId: {UserId} için işlem bulunamadı (direct DbContext).", userId);
                }
                return transactions;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "FinancePlugin: GetAllTransactionsAsync yetkisiz erişim (direct DbContext).");
                return new List<TransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinancePlugin: GetAllTransactionsAsync sırasında hata (direct DbContext).");
                return new List<TransactionDto>();
            }
        }

        [KernelFunction, Description("Kullanıcının belirli bir kategori türündeki (örneğin 'Gelir' veya 'Gider') tüm işlemlerini listeler. LLM GÖREVİ: Kullanıcıdan kategori türünü al (Gelir/Gider), bu fonksiyonu çağır ve sonucu özetle.")]
        public async Task<List<TransactionDto>> GetTransactionsByCategoryTypeAsync(
            [Description("Listelenecek işlemlerin kategori türü. Geçerli değerler: 'Gelir', 'Gider'.")] string categoryType)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("FinancePlugin: GetTransactionsByCategoryTypeAsync çağrıldı (direct DbContext). UserId: {UserId}, CategoryType: {CategoryType}", userId, categoryType);

                var transactions = await _context.Transactions
                    .AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId && t.Category.Type.ToString().Equals(categoryType, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.TransactionDateUtc)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CategoryType = t.Category.Type, // DTO'daki alan string ise
                        UserId = t.UserId,
                        AccountId = t.AccountId,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        TransactionDateUtc = t.TransactionDateUtc,
                        Description = t.Description
                    })
                    .ToListAsync();

                if (!transactions.Any())
                {
                    _logger.LogInformation("FinancePlugin: GetTransactionsByCategoryTypeAsync - UserId: {UserId}, CategoryType: {CategoryType} için işlem bulunamadı (direct DbContext).", userId, categoryType);
                }
                return transactions;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "FinancePlugin: GetTransactionsByCategoryTypeAsync yetkisiz erişim (direct DbContext). CategoryType: {CategoryType}", categoryType);
                return new List<TransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinancePlugin: GetTransactionsByCategoryTypeAsync sırasında hata (direct DbContext). CategoryType: {CategoryType}", categoryType);
                return new List<TransactionDto>();
            }
        }

        [KernelFunction, Description("Kullanıcının belirli bir kategori adına sahip (örneğin 'Maaş', 'Market Alışverişi') tüm işlemlerini listeler. LLM GÖREVİ: Kullanıcıdan kategori adını al, bu fonksiyonu çağır ve sonucu özetle.")]
        public async Task<List<TransactionDto>> GetTransactionsByCategoryNameAsync(
            [Description("Listelenecek işlemlerin kategori adı. Örneğin: 'Maaş', 'Yemek', 'Ulaşım'.")] string categoryName)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("FinancePlugin: GetTransactionsByCategoryNameAsync çağrıldı (direct DbContext). UserId: {UserId}, CategoryName: {CategoryName}", userId, categoryName);

                var transactions = await _context.Transactions
                    .AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId && t.Category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.TransactionDateUtc)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CategoryType = t.Category.Type, // DTO'daki alan string ise
                        UserId = t.UserId,
                        AccountId = t.AccountId,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        TransactionDateUtc = t.TransactionDateUtc,
                        Description = t.Description
                    })
                    .ToListAsync();

                if (!transactions.Any())
                {
                    _logger.LogInformation("FinancePlugin: GetTransactionsByCategoryNameAsync - UserId: {UserId}, CategoryName: {CategoryName} için işlem bulunamadı (direct DbContext).", userId, categoryName);
                }
                return transactions;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "FinancePlugin: GetTransactionsByCategoryNameAsync yetkisiz erişim (direct DbContext). CategoryName: {CategoryName}", categoryName);
                return new List<TransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinancePlugin: GetTransactionsByCategoryNameAsync sırasında hata (direct DbContext). CategoryName: {CategoryName}", categoryName);
                return new List<TransactionDto>();
            }
        }
    }
}
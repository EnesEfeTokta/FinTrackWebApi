using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Services.DocumentService.Generations;
using FinTrackWebApi.Services.DocumentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Reports
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IDocumentGenerationService _documentService;
        private readonly MyDataContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IDocumentGenerationService documentService,
            MyDataContext context,
            ILogger<ReportsController> logger
        )
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private int GetAuthenticatedId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
            {
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid.");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return id;
        }

        [HttpGet("account")]
        public async Task<IActionResult> GetAccountReport(
            [FromQuery] string format,
            [FromQuery] AccountType? type = null,
            [FromQuery] decimal? minBalance = null,
            [FromQuery] decimal? maxBalance = null)
        {
            var reportData = await BuildAccountReportDataAsync(type, minBalance, maxBalance);
            return await GenerateAndReturnReport(
                format,
                DocumentType.Account,
                reportData,
                $"Financial_Account_Report_{DateTime.Now:yyyyMMdd}"
            );
        }

        [HttpGet("budget")]
        public async Task<IActionResult> GetBudgetReport(
            [FromQuery] string format,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var reportData = await BuildBudgetReportDataAsync(startDate, endDate);
            return await GenerateAndReturnReport(
                format,
                DocumentType.Budget,
                reportData,
                $"Financial_Budget_Report_{DateTime.Now:yyyyMMdd}"
            );
        }

        [HttpGet("transaction")]
        public async Task<IActionResult> GetTransactionReport(
            [FromQuery] string format,
            [FromQuery] int? accountId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var reportData = await BuildTransactionReportDataAsync(date, startDate, endDate, accountId, categoryId);
            return await GenerateAndReturnReport(
                format,
                DocumentType.Transaction,
                reportData,
                $"Financial_Transaction_Report_{DateTime.Now:yyyyMMdd}"
            );
        }

        private async Task<IActionResult> GenerateAndReturnReport(
            string format,
            DocumentType documentType,
            IReportModel? reportData,
            string baseFileName
        )
        {
            //if (reportData == null || !reportData.GetType().GetProperty("Items").GetValue(reportData).As<System.CollectionsIList>().Count > 0)
            //{
            //    return NotFound("No report data found for the specified criteria.");
            //}

            if (!Enum.TryParse(format, true, out Services.DocumentService.DocumentFormat requestedFormat))
            {
                return BadRequest("Invalid format. Supported: Pdf, Word, Xlsx, Xml, Text, Markdown");
            }

            try
            {
                var (content, mimeType, fileName) = await _documentService.GenerateDocumentAsync(
                    reportData,
                    requestedFormat,
                    documentType,
                    baseFileName
                );

                return File(content, mimeType, fileName);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Format generation not supported: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report in format {Format}", requestedFormat);
                return StatusCode(500, $"An error occurred while generating the {format} report.");
            }
        }

        private async Task<AccountReportModel?> BuildAccountReportDataAsync(AccountType? type, decimal? minBalance, decimal? maxBalance)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Accounts.Where(a => a.User.Id == userId);

            if (type.HasValue)
            {
                query = query.Where(a => a.Type == type.Value);
            }
            if (minBalance.HasValue)
            {
                query = query.Where(a => a.Balance >= minBalance.Value);
            }
            if (maxBalance.HasValue)
            {
                query = query.Where(a => a.Balance <= maxBalance.Value);
            }

            var accounts = await query.OrderBy(a => a.Name).AsNoTracking().ToListAsync();

            return new AccountReportModel
            {
                ReportTitle = "Financial Account Report",
                Description = $"A detailed report of financial accounts for user {user.UserName}.",
                Items = accounts.Select(a => new AccountReportTableItem
                {
                    Name = a.Name,
                    Type = a.Type,
                    Balance = a.Balance,
                    CreatedAt = a.CreatedAtUtc,
                    UpdatedAt = a.UpdatedAtUtc ?? DateTime.MinValue,
                }).ToList(),
                AccountCount = accounts.Count,
                TotalBalance = accounts.Sum(a => a.Balance)
            };
        }

        private async Task<BudgetReportModel?> BuildBudgetReportDataAsync(DateTime? startDate, DateTime? endDate)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Budgets.Include(b => b.Category).Where(b => b.UserId == userId);

            if (startDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(b => b.EndDate >= startUtc);
            }
            if (endDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
                query = query.Where(b => b.StartDate <= endUtc);
            }

            var budgets = await query.OrderBy(b => b.StartDate).ThenBy(b => b.Name).AsNoTracking().ToListAsync();

            return new BudgetReportModel
            {
                ReportTitle = "Financial Budget Report",
                Description = $"Budget details for user {user.UserName}.",
                Items = budgets.Select(b => new BudgetReportTableItem
                {
                    Name = b.Name,
                    Description = b.Description ?? "-",
                    Category = b.Category.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    CreatedAt = b.CreatedAtUtc,
                    UpdatedAt = b.UpdatedAtUtc ?? DateTime.MinValue,
                    AllocatedAmount = b.AllocatedAmount,
                }).ToList(),
                BudgetCount = budgets.Count,
                TotalAllocatedAmount = budgets.Sum(b => b.AllocatedAmount)
            };
        }

        private async Task<TransactionsRaportModel?> BuildTransactionReportDataAsync(DateTime? date, DateTime? startDate, DateTime? endDate, int? accountId, int? categoryId)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.User.Id == userId);

            if (date.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                query = query.Where(t => t.TransactionDateUtc == startUtc);
            }
            if (startDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(t => t.TransactionDateUtc >= startUtc);
            }
            if (endDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionDateUtc <= endUtc);
            }
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            var transactions = await query.OrderByDescending(t => t.TransactionDateUtc).AsNoTracking().ToListAsync();

            return new TransactionsRaportModel
            {
                ReportTitle = "Financial Transaction Report",
                Description = $"Transaction details for user {user.UserName}.",
                Items = transactions.Select(t => new TransactionRaportTableItem
                {
                    AccountName = t.Account.Name,
                    Description = t.Description ?? "-",
                    CategoryName = t.Category.Name,
                    Amount = t.Amount,
                    TransactionDateUtc = t.TransactionDateUtc,
                }).ToList(),
                TransactionCount = transactions.Count,
                TotalAmount = transactions.Sum(t => t.Amount)
            };
        }
    }
}
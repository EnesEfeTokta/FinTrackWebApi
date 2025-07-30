using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.ReportDtos;
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

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Report request cannot be null.");
            }

            IReportModel? reportData = null;
            DocumentType documentType;

            switch (request.ReportType)
            {
                case ReportType.Transaction:
                    reportData = await BuildTransactionReportDataAsync(request);
                    documentType = DocumentType.Transaction;
                    break;

                case ReportType.Account:
                    reportData = await BuildAccountReportDataAsync(request);
                    documentType = DocumentType.Account;
                    break;

                case ReportType.Budget:
                    reportData = await BuildBudgetReportDataAsync(request);
                    documentType = DocumentType.Budget;
                    break;

                default:
                    return BadRequest("Unsupported report type.");
            }

            if (reportData == null || reportData.Items.Count == 0)
            {
                return NotFound("No report data found for the specified criteria.");
            }

            string fileName = $"Financial_{request.ReportType}_Report_{DateTime.Now:yyyyMMdd}";

            return await GenerateAndReturnReport(request.ExportFormat, documentType, reportData, fileName);
        }

        private async Task<IActionResult> GenerateAndReturnReport(
            Enums.DocumentFormat requestedFormat,
            DocumentType documentType,
            IReportModel reportData,
            string baseFileName
        )
        {
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
                return StatusCode(500, $"An error occurred while generating the {requestedFormat} report.");
            }
        }

        private async Task<AccountReportModel?> BuildAccountReportDataAsync(ReportRequestDto request)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Accounts.Where(a => a.User.Id == userId);

            if (request.SelectedAccountIds != null && request.SelectedAccountIds.Any())
            {
                query = query.Where(a => request.SelectedAccountIds.Contains(a.Id));
            }

            if (request.MinBalance.HasValue)
            {
                query = query.Where(a => a.Balance >= request.MinBalance.Value);
            }

            if (request.MaxBalance.HasValue)
            {
                query = query.Where(a => a.Balance <= request.MaxBalance.Value);
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

        private async Task<BudgetReportModel?> BuildBudgetReportDataAsync(ReportRequestDto request)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Budgets.Include(b => b.Category).Where(b => b.UserId == userId);

            if (request.SelectedBudgetIds != null && request.SelectedBudgetIds.Any())
            {
                query = query.Where(b => request.SelectedBudgetIds.Contains(b.Id));
            }

            if (request.StartDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(b => b.EndDate >= startUtc);
            }
            if (request.EndDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(request.EndDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
                query = query.Where(b => b.StartDate <= endUtc);
            }

            if (request.SelectedCategoryIds != null && request.SelectedCategoryIds.Any())
            {
                query = query.Where(b => request.SelectedCategoryIds.Contains(b.CategoryId));
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

        private async Task<TransactionsRaportModel?> BuildTransactionReportDataAsync(ReportRequestDto request)
        {
            int userId = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var query = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.User.Id == userId);

            if (request.Date.HasValue)
            {
                var dayStartUtc = DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc);
                var dayEndUtc = dayStartUtc.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionDateUtc >= dayStartUtc && t.TransactionDateUtc <= dayEndUtc);
            }
            else
            {
                if (request.StartDate.HasValue)
                {
                    var startUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(t => t.TransactionDateUtc >= startUtc);
                }
                if (request.EndDate.HasValue)
                {
                    var endUtc = DateTime.SpecifyKind(request.EndDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
                    query = query.Where(t => t.TransactionDateUtc <= endUtc);
                }
            }

            if (request.SelectedAccountIds != null && request.SelectedAccountIds.Any())
            {
                query = query.Where(t => request.SelectedAccountIds.Contains(t.AccountId));
            }

            if (request.SelectedCategoryIds != null && request.SelectedCategoryIds.Any())
            {
                query = query.Where(t => request.SelectedCategoryIds.Contains(t.CategoryId));
            }

            if (request.IsIncomeSelected && !request.IsExpenseSelected)
            {
                query = query.Where(t => t.Amount > 0);
            }
            else if (!request.IsIncomeSelected && request.IsExpenseSelected)
            {
                query = query.Where(t => t.Amount < 0);
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
using FinTrackWebApi.Data;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Services.DocumentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Reports
{
    [Route("api/[controller]")]
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
            _documentService =
                documentService ?? throw new ArgumentNullException(nameof(documentService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// JWT token ile kimlik doğrulaması yapılmış kullanıcının ID'sini alır.
        /// </summary>
        private int GetAuthenticatedId()
        {
            var IdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(IdClaim) || !int.TryParse(IdClaim, out int Id))
            {
                _logger.LogError(
                    "Authenticated user ID claim (NameIdentifier) not found or invalid."
                );
                throw new UnauthorizedAccessException(
                    "User ID cannot be determined from the token."
                );
            }
            return Id;
        }

        #region Budget Endpoints
        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen formatta döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <returns></returns>
        [HttpGet("budget-report/{format}")]
        public async Task<IActionResult> GetBudgetReport(string format)
        {
            var reportData = await BuildBudgetReportDataAsync();
            return await GenerateAndReturnReport(
                format,
                DocumentType.Budget.ToString(),
                reportData,
                $"Financial_Budget_Report_{DateTime.Now:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen tarih aralığında döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="startDateTime">Başlangıç tarihi</param>
        /// <param name="endDateTime">Bitiş tarihi</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-date/{format}")]
        public async Task<IActionResult> GetBudgetReportByDate(
            [FromQuery] string format,
            DateTime startDateTime,
            DateTime endDateTime
        )
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest("Start date must be before end date.");
            }

            var reportData = await BuildBudgetReportDataAsync(startDateTime, endDateTime);

            return await GenerateAndReturnReport(
                format,
                DocumentType.Budget.ToString(),
                reportData,
                $"Financial_Budget_Report_{startDateTime:yyyyMMdd}_{endDateTime:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen kategoriye göre döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="category">İstenilen kategori</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-category/{category}")]
        public async Task<IActionResult> GetBudgetReportByCategory(
            [FromQuery] string format,
            string category
        )
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Category cannot be null or empty.");
            }

            var reportData = await BuildBudgetReportDataAsync();
            reportData.Items = reportData
                .Items.Where(item =>
                    item.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            return await GenerateAndReturnReport(
                format,
                DocumentType.Budget.ToString(),
                reportData,
                $"Financial_Budget_Report_By_Category_{category}_{DateTime.Now:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen türde döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="type">İstenilen tür</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-type/{type}")]
        public async Task<IActionResult> GetBudgetReportByType(
            [FromQuery] string format,
            string type
        )
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Type cannot be null or empty.");
            }

            var reportData = await BuildBudgetReportDataAsync();

            return await GenerateAndReturnReport(
                format,
                DocumentType.Budget.ToString(),
                reportData,
                $"Financial_Budget_Report_By_Type_{type}_{DateTime.Now:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Başlangıç ve bitiş tarihleri belirtilirse, o tarihler arasındaki bütçeleri getirir.
        /// </summary>
        /// <param name="start">Başlangıç tarihi</param>
        /// <param name="end">Biriş tarihi</param>
        /// <returns></returns>
        private async Task<BudgetReportModel?> BuildBudgetReportDataAsync(
            DateTime? start = null,
            DateTime? end = null
        )
        {
            int Id = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null)
            {
                _logger.LogError("User not found for ID: {Id} while building report.", Id);
                return null;
            }

            var report = new BudgetReportModel
            {
                ReportTitle =
                    start.HasValue && end.HasValue
                        ? $"Financial Budget Report ({start.Value:yyyy-MM-dd} - {end.Value:yyyy-MM-dd})"
                        : "Financial Budget Report (All Time)",
                Description = $"Budget details for user {user.UserName}.",
            };

            DateTime? startUtc = start.HasValue
                ? DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc)
                : null;
            DateTime? endUtc = end.HasValue
                ? DateTime.SpecifyKind(end.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1)
                : null;

            var query = _context
                .BudgetCategories.Include(bc => bc.Budget)
                .Include(bc => bc.Category)
                .Where(bc => bc.Budget.UserId == Id);

            if (startUtc.HasValue && endUtc.HasValue)
            {
                query = query.Where(bc =>
                    bc.Budget.EndDate >= startUtc.Value && bc.Budget.StartDate <= endUtc.Value
                );
            }

            var budgetCategoriesData = await query
                .OrderBy(bc => bc.Budget.StartDate)
                .ThenBy(bc => bc.Budget.Name)
                .ThenBy(bc => bc.Category.Name)
                .AsNoTracking()
                .ToListAsync();

            if (!budgetCategoriesData.Any())
            {
                _logger.LogInformation(
                    "No budget categories found for user {UserName} for the given criteria.",
                    user.UserName
                );
                return report;
            }

            report.Items = budgetCategoriesData
                .Select(bc => new BudgetReportTableItem
                {
                    Name = bc.Budget.Name,
                    Description = bc.Budget.Description ?? "-",
                    Category = bc.Category.Name,
                    StartDate = bc.Budget.StartDate,
                    EndDate = bc.Budget.EndDate,
                    CreatedAt = bc.Budget.CreatedAtUtc,
                    UpdatedAt = bc.Budget.UpdatedAtUtc ?? DateTime.MinValue,
                    AllocatedAmount = bc.AllocatedAmount,
                })
                .ToList();

            return report;
        }

        /// <summary>
        /// Belirtilen formatta raporu oluşturur ve döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="reportData">Raporun detayları</param>
        /// <param name="baseFileName">Dosyanın ismi</param>
        /// <returns></returns>
        private async Task<IActionResult> GenerateAndReturnReport(
            string format,
            string type,
            BudgetReportModel? reportData,
            string baseFileName
        )
        {
            if (reportData == null || !reportData.Items.Any())
            {
                return NotFound("No report data found for the specified criteria.");
            }

            if (
                !Enum.TryParse(
                    format,
                    true,
                    out Services.DocumentService.DocumentFormat requestedFormat
                )
            )
            {
                return BadRequest(
                    "Invalid or unsupported format requested. Supported formats: Pdf, Word, Text, Markdown"
                );
            }

            if (!Enum.TryParse(type, true, out DocumentType requestedType))
            {
                return BadRequest(
                    "Invalid or unsupported type requested. Supported types: Budget, Transaction, Account"
                );
            }

            try
            {
                var (content, mimeType, fileName) = await _documentService.GenerateDocumentAsync(
                    reportData,
                    requestedFormat,
                    requestedType,
                    baseFileName
                );

                return File(content, mimeType, fileName);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Format generation not supported: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid data for report generation: {Message}", ex.Message);
                return BadRequest($"Invalid data for {format} report generation: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report in format {Format}", requestedFormat);
                return StatusCode(500, $"An error occurred while generating the {format} report.");
            }
        }
        #endregion

        #region Transaction Endpoints
        /// <summary>
        /// Kullanıcının gelir giderlerin raporunu belirtilen formatta döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <returns></returns>
        [HttpGet("transaction-report/{format}")]
        public async Task<IActionResult> GetTransactionReport(string format)
        {
            var reportData = await BuildTransactionReportDataAsync();
            return await GenerateAndReturnReport(
                format,
                DocumentType.Transaction.ToString(),
                reportData,
                $"Financial_Transaction_Report_{DateTime.Now:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Kullanıcının  gelir giderlerin raporunu belirtilen tarih aralığında döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="startDateTime">Başlangıç tarihi</param>
        /// <param name="endDateTime">Bitiş tarihi</param>
        /// <returns></returns>
        [HttpGet("transaction-report-by-date/{format}")]
        public async Task<IActionResult> GetTransactionReportByDate(
            [FromQuery] string format,
            DateTime startDateTime,
            DateTime endDateTime
        )
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest("Start date must be before end date.");
            }

            var reportData = await BuildTransactionReportDataAsync(startDateTime, endDateTime);

            return await GenerateAndReturnReport(
                format,
                DocumentType.Transaction.ToString(),
                reportData,
                $"Financial_Transaction_Report_{startDateTime:yyyyMMdd}_{endDateTime:yyyyMMdd}"
            );
        }

        /// <summary>
        /// Belirtilen formatta raporu oluşturur ve döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="reportData">Raporun detayları</param>
        /// <param name="baseFileName">Dosyanın ismi</param>
        /// <returns></returns>
        private async Task<IActionResult> GenerateAndReturnReport(
            string format,
            string type,
            TransactionsRaportModel? reportData,
            string baseFileName
        )
        {
            if (reportData == null || !reportData.Items.Any())
            {
                return NotFound("No report data found for the specified criteria.");
            }

            if (
                !Enum.TryParse(
                    format,
                    true,
                    out Services.DocumentService.DocumentFormat requestedFormat
                )
            )
            {
                return BadRequest(
                    "Invalid or unsupported format requested. Supported formats: Pdf, Word, Text, Markdown"
                );
            }

            if (!Enum.TryParse(type, true, out DocumentType requestedType))
            {
                return BadRequest(
                    "Invalid or unsupported type requested. Supported types: Budget, Transaction, Account"
                );
            }

            try
            {
                var (content, mimeType, fileName) = await _documentService.GenerateDocumentAsync(
                    reportData,
                    requestedFormat,
                    requestedType,
                    baseFileName
                );

                return File(content, mimeType, fileName);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Format generation not supported: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid data for report generation: {Message}", ex.Message);
                return BadRequest($"Invalid data for {format} report generation: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report in format {Format}", requestedFormat);
                return StatusCode(500, $"An error occurred while generating the {format} report.");
            }
        }

        /// <summary>
        /// Başlangıç ve bitiş tarihleri belirtilirse, o tarihler arasındaki gelir ve giderleri getirir.
        /// </summary>
        /// <param name="start">Başlangıç tarihi</param>
        /// <param name="end">Biriş tarihi</param>
        /// <returns></returns>
        private async Task<TransactionsRaportModel?> BuildTransactionReportDataAsync(
            DateTime? start = null,
            DateTime? end = null
        )
        {
            int Id = GetAuthenticatedId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null)
            {
                _logger.LogError("User not found for ID: {Id} while building report.", Id);
                return null;
            }

            var report = new TransactionsRaportModel
            {
                ReportTitle =
                    start.HasValue && end.HasValue
                        ? $"Financial Transaction Report ({start.Value:yyyy-MM-dd} - {end.Value:yyyy-MM-dd})"
                        : "Financial Transaction Report (All Time)",
                Description = $"Transaction details for user {user.UserName}.",
            };

            DateTime? startUtc = start.HasValue
                ? DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc)
                : null;
            DateTime? endUtc = end.HasValue
                ? DateTime.SpecifyKind(end.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1)
                : null;

            var query = _context
                .Transactions.Include(bc => bc.Account)
                .Include(bc => bc.Category)
                .Where(bc => bc.User.Id == Id);

            if (startUtc.HasValue && endUtc.HasValue)
            {
                query = query.Where(bc =>
                    bc.TransactionDateUtc >= startUtc.Value && bc.TransactionDateUtc <= endUtc.Value
                );
            }

            var transactionCategoriesData = await query
                .OrderBy(bc => bc.TransactionDateUtc)
                .ThenBy(bc => bc.Account.Name)
                .ThenBy(bc => bc.Category.Name)
                .AsNoTracking()
                .ToListAsync();

            if (!transactionCategoriesData.Any())
            {
                _logger.LogInformation(
                    "No transaction categories found for user {UserName} for the given criteria.",
                    user.UserName
                );
                return report;
            }

            report.Items = transactionCategoriesData
                .Select(bc => new TransactionRaportTableItem
                {
                    AccountName = bc.Account.Name,
                    Description = bc.Description ?? "-",
                    CategoryName = bc.Category.Name,
                    Amount = bc.Amount,
                    TransactionDateUtc = bc.TransactionDateUtc,
                })
                .ToList();

            return report;
        }
        #endregion
    }
}

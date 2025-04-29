using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IDocumentGenerationService _documentService;
        private readonly MyDataContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IDocumentGenerationService documentService, MyDataContext context, ILogger<ReportsController> logger)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// JWT token ile kimlik doğrulaması yapılmış kullanıcının ID'sini alır.
        /// </summary>
        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid.");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen formatta döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <returns></returns>
        [HttpGet("budget-report/{format}")]
        public async Task<IActionResult> GetBudgetReport(string format)
        {
            var reportData = await BuildReportDataAsync();
            return await GenerateAndReturnReport(format, reportData, $"Financial_Budget_Report_{DateTime.Now:yyyyMMdd}");
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen tarih aralığında döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="startDateTime">Başlangıç tarihi</param>
        /// <param name="endDateTime">Bitiş tarihi</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-date/{format}")]
        public async Task<IActionResult> GetBudgetReportByDate([FromQuery] string format, DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest("Start date must be before end date.");
            }

            var reportData = await BuildReportDataAsync(startDateTime, endDateTime);

            return await GenerateAndReturnReport(format, reportData, $"Financial_Budget_Report_{startDateTime:yyyyMMdd}_{endDateTime:yyyyMMdd}");
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen kategoriye göre döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="category">İstenilen kategori</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-category/{category}")]
        public async Task<IActionResult> GetBudgetReportByCategory([FromQuery] string format, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Category cannot be null or empty.");
            }

            var reportData = await BuildReportDataAsync();
            reportData.Items = reportData.Items.Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            return await GenerateAndReturnReport(format, reportData, $"Financial_Budget_Report_By_Category_{category}_{DateTime.Now:yyyyMMdd}");
        }

        /// <summary>
        /// Kullanıcının bütçe raporunu belirtilen türde döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="type">İstenilen tür</param>
        /// <returns></returns>
        [HttpGet("budget-report-by-type/{type}")]
        public async Task<IActionResult> GetBudgetReportByType([FromQuery] string format, string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Type cannot be null or empty.");
            }

            var reportData = await BuildReportDataAsync();
            reportData.Items = reportData.Items.Where(item => item.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

            return await GenerateAndReturnReport(format, reportData, $"Financial_Budget_Report_By_Type_{type}_{DateTime.Now:yyyyMMdd}");
        }

        /// <summary>
        /// Belirtilen formatta raporu oluşturur ve döndürür.
        /// </summary>
        /// <param name="format">Çıktı formatı</param>
        /// <param name="reportData">Raporun detayları</param>
        /// <param name="baseFileName">Dosyanın ismi</param>
        /// <returns></returns>
        private async Task<IActionResult> GenerateAndReturnReport(string format, BudgetReportModel? reportData, string baseFileName)
        {
            if (reportData == null || !reportData.Items.Any())
            {
                return NotFound("No report data found for the specified criteria.");
            }

            if (!Enum.TryParse(format, true, out Services.DocumentService.DocumentFormat requestedFormat))
            {
                return BadRequest("Invalid or unsupported format requested. Supported formats: Pdf, Word, Text.");
            }

            try
            {
                var (content, mimeType, fileName) = await _documentService.GenerateDocumentAsync(
                    reportData,
                    requestedFormat,
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
        /// Rapor verilerini oluşturur.
        /// </summary>
        /// <param name="start">Başlangıç tarihi</param>
        /// <param name="end">Biriş tarihi</param>
        /// <returns></returns>
        private async Task<BudgetReportModel?> BuildReportDataAsync(DateTime? start = null, DateTime? end = null)
        {
            int userId = GetAuthenticatedUserId();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogError("User not found for ID: {UserId} while building report.", userId);
                return null;
            }

            var report = new BudgetReportModel
            {
                ReportTitle = start.HasValue && end.HasValue
                    ? $"Financial Budget Report ({start.Value:yyyy-MM-dd} - {end.Value:yyyy-MM-dd})"
                    : "Financial Budget Report (All Time)",
                Description = $"Budget details for user {user.Username}."
            };

            DateTime? startUtc = start.HasValue ? DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endUtc = end.HasValue ? DateTime.SpecifyKind(end.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1) : null;

            var query = _context.BudgetCategories
                .Include(bc => bc.Budget)
                .Include(bc => bc.Category)
                .Where(bc => bc.Budget.UserId == userId);

            if (startUtc.HasValue && endUtc.HasValue)
            {
                query = query.Where(bc => bc.Budget.EndDate >= startUtc.Value &&
                                          bc.Budget.StartDate <= endUtc.Value);
            }

            var budgetCategoriesData = await query
                .OrderBy(bc => bc.Budget.StartDate)
                .ThenBy(bc => bc.Budget.Name)
                .ThenBy(bc => bc.Category.Name)
                .AsNoTracking()
                .ToListAsync();

            if (!budgetCategoriesData.Any())
            {
                _logger.LogInformation("No budget categories found for user {UserName} for the given criteria.", user.Username);
                return report;
            }

            report.Items = budgetCategoriesData.Select(bc => new BudgetReportTableItem // DTO Adını kontrol et
            {
                Name = bc.Budget.Name,
                Description = bc.Budget.Description ?? "-",
                Category = bc.Category.Name,
                Type = bc.Category.Type.ToString(),
                StartDate = bc.Budget.StartDate,
                EndDate = bc.Budget.EndDate,
                CreatedAt = bc.Budget.CreatedAtUtc,
                UpdatedAt = bc.Budget.UpdatedAtUtc ?? DateTime.MinValue,
                AllocatedAmount = bc.AllocatedAmount
            }).ToList();

            return report;
        }
    }
}

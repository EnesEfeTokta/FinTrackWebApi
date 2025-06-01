using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Dtos;

namespace FinTrackWebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class DebtController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<DebtController> _logger;

        public DebtController(MyDataContext context, ILogger<DebtController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return userId;
        }

        // Kullanıcı borçlarını listeleme metodu.
        [HttpGet("user-debts")]
        public async Task<IActionResult> GetUserDebtsAsync()
        {
            int userId = GetAuthenticatedUserId();
            try
            {
                var debts = await _context.Debts
                    .Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.VideoMetadata)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.Lender.UserId == userId || d.Borrower.UserId == userId)
                    .ToListAsync();

                return Ok(debts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user debts.");
                return StatusCode(500, "Internal server error while retrieving debts.");
            }
        }

        // Borç teklifi oluşturma metodu.
        [HttpPost("create-debt-offer")]
        public async Task<IActionResult> CreateDebtOfferAsync([FromBody] CreateDebtOfferRequestDto request)
        {
            if (request == null || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Description) || request.DueDateUtc == default)
            {
                return BadRequest("Invalid debt offer request.");
            }

            int userId = GetAuthenticatedUserId();
            var borrower = await _context.Users.FindAsync(request.BorrowerId);
            if (borrower == null || borrower.UserId == userId)
            {
                return BadRequest("Invalid borrower specified.");
            }

            var lender = await _context.Users.FindAsync(userId);
            if (lender == null)
            {
                return Unauthorized("Lender not found.");
            }

            var debt = new DebtModel
            {
                LenderId = lender.UserId,
                BorrowerId = borrower.UserId,
                Amount = request.Amount,
                CurrencyModel = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == request.CurrencyCode) ?? throw new ArgumentException("Invalid currency code."),
                DueDateUtc = DateTime.SpecifyKind(request.DueDateUtc, DateTimeKind.Unspecified).ToUniversalTime(),
                Description = request.Description,
                CreateAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Status = DebtStatus.PendingBorrowerAcceptance
            };

            try
            {
                _context.Debts.Add(debt);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating debt offer.");
                return StatusCode(500, "Internal server error while creating debt offer.");
            }

            return CreatedAtAction(nameof(GetUserDebtsAsync), new { id = debt.DebtId }, debt);
        }
    }
}

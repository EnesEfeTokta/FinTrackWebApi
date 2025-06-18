using System.Security.Claims;
using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.SecureDebtService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class DebtController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<DebtController> _logger;
        private readonly ISecureDebtService _secureDebtService;

        public DebtController(
            MyDataContext context,
            ILogger<DebtController> logger,
            ISecureDebtService secureDebtService
        )
        {
            _context = context;
            _logger = logger;
            _secureDebtService = secureDebtService;
        }

        private int GetAuthenticatedId()
        {
            var IdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(IdClaim, out int Id))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return Id;
        }

        // Kullanıcı borçlarını listeleme metodu.
        [HttpGet("user-debts")]
        public async Task<IActionResult> GetUserDebtsAsync()
        {
            int Id = GetAuthenticatedId();
            try
            {
                var debts = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.Lender.Id == Id || d.Borrower.Id == Id)
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
        public async Task<IActionResult> CreateDebtOfferAsync(
            [FromBody] CreateDebtOfferRequestDto request
        )
        {
            if (
                request == null
                || request.Amount <= 0
                || string.IsNullOrWhiteSpace(request.Description)
                || request.DueDateUtc == default
            )
            {
                return BadRequest("Invalid debt offer request.");
            }

            int Id = GetAuthenticatedId();

            var borrower = await _context.Users.FindAsync(request.BorrowerId);
            if (borrower == null || borrower.Id == Id)
            {
                return BadRequest("Invalid borrower specified.");
            }

            var lender = await _context.Users.FindAsync(Id);
            if (lender == null)
            {
                return Unauthorized("Lender not found.");
            }

            try
            {
                CurrencyModel currencyModel =
                    await _context.Currencies.FirstOrDefaultAsync(c =>
                        c.Code == request.CurrencyCode
                    ) ?? throw new ArgumentException("Invalid currency code.");

                await _secureDebtService.CreateDebtOfferAsync(
                    lender.Id.ToString(),
                    borrower.Email
                        ?? throw new ArgumentException("We need the borrower's email address."),
                    request.Amount,
                    currencyModel,
                    request.DueDateUtc,
                    request.Description
                );

                return Ok(
                    new
                    {
                        Message = "Debt offer created successfully.",
                        Debt = new
                        {
                            LenderId = Id,
                            BorrowerId = request.BorrowerId,
                            Amount = request.Amount,
                            CurrencyCode = request.CurrencyCode,
                            DueDateUtc = request.DueDateUtc,
                            Description = request.Description,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating debt offer.");
                return StatusCode(500, "Internal server error while creating debt offer.");
            }
        }

        // Borç bilgilerini alma metodu.
        [HttpGet("debt/{debtId}")]
        public async Task<IActionResult> GetDebtByIdAsync(int debtId)
        {
            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .FirstOrDefaultAsync(d => d.DebtId == debtId);
                if (debt == null)
                {
                    return NotFound("Debt not found.");
                }
                return Ok(debt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debt by ID.");
                return StatusCode(500, "Internal server error while retrieving debt.");
            }
        }

        // Kullanıcıya ait borçları alma metodu.
        [HttpGet("user-debts/{Id}")]
        public async Task<IActionResult> GetDebtsByUserIdAsync(int Id)
        {
            try
            {
                var debts = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.Lender.Id == Id || d.Borrower.Id == Id)
                    .ToListAsync();
                return Ok(debts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debts by user ID.");
                return StatusCode(500, "Internal server error while retrieving debts.");
            }
        }

        // Borç teklifini kabul etme metodu.
        [HttpPost("accept-debt-offer/debt-{debtId}/decision-{decision}")]
        public async Task<IActionResult> AcceptDebtOfferAsync(int debtId, bool decision)
        {
            int Id = GetAuthenticatedId();
            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .FirstOrDefaultAsync(d => d.DebtId == debtId);
                if (debt == null)
                {
                    return NotFound("Debt offer not found.");
                }
                if (debt.Borrower.Id != Id)
                {
                    return Forbid("You are not authorized to accept this debt offer.");
                }

                if (debt.Status != DebtStatus.PendingBorrowerAcceptance)
                {
                    return BadRequest("Debt offer is not in a state that can be accepted.");
                }

                if (decision)
                {
                    debt.Status = DebtStatus.PendingOperatorApproval;
                    debt.UpdatedAtUtc = DateTime.UtcNow;

                    _context.Debts.Update(debt);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    debt.Status = DebtStatus.RejectedByBorrower;
                    debt.UpdatedAtUtc = DateTime.UtcNow;

                    _context.Debts.Update(debt);
                    await _context.SaveChangesAsync();
                }

                return Ok(
                    new
                    {
                        Message = "Debt offer retrieved successfully.",
                        Debt = new
                        {
                            debt.DebtId,
                            debt.Lender,
                            debt.Borrower,
                            debt.Amount,
                            debt.CurrencyModel?.Code,
                            debt.DueDateUtc,
                            debt.Description,
                            Status = debt.Status.ToString(),
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debt offer for acceptance.");
                return StatusCode(500, "Internal server error while retrieving debt offer.");
            }
        }
    }
}

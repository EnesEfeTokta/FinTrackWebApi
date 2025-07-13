using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.DebtDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.SecureDebtService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Debts
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
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

        // Kullanıcı borçlarını(borçlu veya alacaklı) listeleme metodu.
        [HttpGet]
        public async Task<IActionResult> GetUserDebtsAsync()
        {
            int userId = GetAuthenticatedId();
            try
            {
                var debtsList = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.Currency)
                    .Where(d => d.Lender.Id == userId || d.Borrower.Id == userId)
                    .ToListAsync();

                if (debtsList == null || debtsList.Count == 0)
                {
                    _logger.LogInformation(
                        "No debts found for user with ID {UserId}.",
                        userId
                    );
                    return NotFound("No debts found for the user.");
                }

                var debtDtos = new List<DebtDto>();
                foreach (var debt in debtsList)
                {
                    var debtDto = new DebtDto
                    {
                        Id = debt.Id,
                        LenderId = debt.Lender.Id,
                        LenderName = debt.Lender.UserName,
                        LenderEmail = debt.Lender.Email,
                        LenderProfilePicture = debt.Lender.ProfilePicture,
                        BorrowerId = debt.Borrower.Id,
                        BorrowerName = debt.Borrower.UserName,
                        BorrowerEmail = debt.Borrower.Email,
                        BorrowerProfilePicture = debt.Borrower.ProfilePicture,
                        Amount = debt.Amount,
                        Currency = debt.Currency,
                        DueDateUtc = debt.DueDateUtc,
                        Description = debt.Description ?? string.Empty,
                        Status = debt.Status,
                        CreateAtUtc = debt.CreateAtUtc,
                        UpdatedAtUtc = debt.UpdatedAtUtc,
                        PaidAtUtc = debt.PaidAtUtc,
                        OperatorApprovalAtUtc = debt.OperatorApprovalAtUtc,
                        BorrowerApprovalAtUtc = debt.BorrowerApprovalAtUtc,
                        PaymentConfirmationAtUtc = debt.PaymentConfirmationAtUtc
                    };
                    debtDtos.Add(debtDto);
                }

                _logger.LogInformation(
                    "Retrieved {DebtCount} debts for user with ID {UserId}.",
                    debtDtos.Count,
                    userId
                );

                return Ok(debtDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user debts.");
                return StatusCode(500, "Internal server error while retrieving debts.");
            }
        }

        // Kullanıcı borçlarını(borçlu veya alacaklı) alma metodu.
        [HttpGet("{debtId}")]
        public async Task<IActionResult> GetDebtByIdAsync(int Id)
        {
            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.Currency)
                    .FirstOrDefaultAsync(d => d.Id == Id);

                if (debt == null)
                {
                    _logger.LogInformation("Debt with ID {DebtId} not found.", Id);
                    return NotFound("Debt not found.");
                }

                var debtDto = new DebtDto
                {
                    Id = debt.Id,
                    LenderId = debt.Lender.Id,
                    LenderName = debt.Lender.UserName,
                    LenderEmail = debt.Lender.Email,
                    LenderProfilePicture = debt.Lender.ProfilePicture,
                    BorrowerId = debt.Borrower.Id,
                    BorrowerName = debt.Borrower.UserName,
                    BorrowerEmail = debt.Borrower.Email,
                    BorrowerProfilePicture = debt.Borrower.ProfilePicture,
                    Amount = debt.Amount,
                    Currency = debt.Currency,
                    DueDateUtc = debt.DueDateUtc,
                    Description = debt.Description ?? string.Empty,
                    Status = debt.Status,
                    CreateAtUtc = debt.CreateAtUtc,
                    UpdatedAtUtc = debt.UpdatedAtUtc,
                    PaidAtUtc = debt.PaidAtUtc,
                    OperatorApprovalAtUtc = debt.OperatorApprovalAtUtc,
                    BorrowerApprovalAtUtc = debt.BorrowerApprovalAtUtc,
                    PaymentConfirmationAtUtc = debt.PaymentConfirmationAtUtc
                };

                _logger.LogInformation(
                    "Retrieved debt with ID {DebtId} for user with ID {UserId}.",
                    Id,
                    GetAuthenticatedId()
                );
                return Ok(debtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debt by ID.");
                return StatusCode(500, "Internal server error while retrieving debt.");
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
                || request.DueDateUtc == default
            )
            {
                _logger.LogWarning(
                    "Invalid debt offer request received: {Request}.",
                    request
                );
                return BadRequest("Invalid debt offer request.");
            }

            int userId = GetAuthenticatedId();

            var borrower = await _context.Users.FindAsync(request.BorrowerEmail);
            if (borrower == null || borrower.Id == userId)
            {
                _logger.LogWarning(
                    "Invalid borrower specified for user with email {Email}.",
                    request.BorrowerEmail
                );
                return BadRequest("Invalid borrower specified.");
            }

            var lender = await _context.Users.FindAsync(userId);
            if (lender == null)
            {
                _logger.LogWarning(
                    "Lender with ID {LenderId} not found.",
                    userId
                );
                return Unauthorized("Lender not found.");
            }
            await _secureDebtService.CreateDebtOfferAsync(
                lender,
                borrower,
                request.Amount,
                request.CurrencyCode,
                request.DueDateUtc,
                request.Description
            );
            return Ok(true);
        }

        // Borç teklifini kabul etme metodu.
        [HttpPost("accept-debt-offer/debt-{debtId}/decision-{decision}")]
        public async Task<IActionResult> AcceptDebtOfferAsync(int debtId, bool decision = false)
        {
            int userId = GetAuthenticatedId();
            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .FirstOrDefaultAsync(d => d.Id == debtId);
                if (debt == null)
                {
                    _logger.LogInformation(
                        "Debt offer with ID {DebtId} not found for user with ID {UserId}.",
                        debtId,
                        userId
                    );
                    return NotFound("Debt offer not found.");
                }
                if (debt.Borrower.Id != userId)
                {
                    _logger.LogWarning(
                        "User with ID {UserId} is not authorized to accept debt offer with ID {DebtId}.",
                        userId,
                        debtId
                    );
                    return Forbid("You are not authorized to accept this debt offer.");
                }

                if (debt.Status != DebtStatusType.PendingBorrowerAcceptance)
                {
                    _logger.LogWarning(
                        "Debt offer with ID {DebtId} is not in a state that can be accepted by user with ID {UserId}.",
                        debtId,
                        userId
                    );
                    return BadRequest("Debt offer is not in a state that can be accepted.");
                }

                if (decision)
                {
                    debt.Status = DebtStatusType.PendingOperatorApproval;
                    debt.BorrowerApprovalAtUtc = DateTime.UtcNow;
                    debt.UpdatedAtUtc = DateTime.UtcNow;

                    _context.Debts.Update(debt);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    debt.Status = DebtStatusType.RejectedByBorrower;
                    debt.UpdatedAtUtc = DateTime.UtcNow;

                    _context.Debts.Update(debt);
                    await _context.SaveChangesAsync();
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debt offer for acceptance.");
                return StatusCode(500, "Internal server error while retrieving debt offer.");
            }
        }
    }
}

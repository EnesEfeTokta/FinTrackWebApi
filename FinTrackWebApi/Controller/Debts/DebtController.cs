using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.DebtDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Services.SecureDebtService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Debts
{
    [ApiController]
    [Route("[controller]")]
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
                        LenderId = debt.Lender?.Id ?? 0,
                        LenderName = debt.Lender?.UserName ?? "N/A",
                        LenderEmail = debt.Lender?.Email ?? "N/A",
                        LenderProfilePicture = debt.Lender?.ProfilePicture ?? "N/A",
                        BorrowerId = debt.Borrower?.Id ?? 0,
                        BorrowerName = debt.Borrower?.UserName ?? "N/A",
                        BorrowerEmail = debt.Borrower?.Email ?? "N/A",
                        BorrowerProfilePicture = debt.Borrower?.ProfilePicture ?? "N/A",
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
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetDebtByIdAsync(int Id)
        {
            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .FirstOrDefaultAsync(d => d.Id == Id);

                if (debt == null)
                {
                    _logger.LogInformation("Debt with ID {DebtId} not found.", Id);
                    return NotFound("Debt not found.");
                }

                var debtDto = new DebtDto
                {
                    Id = debt.Id,
                    LenderId = debt.Lender?.Id ?? 0,
                    LenderName = debt.Lender?.UserName ?? "N/A",
                    LenderEmail = debt.Lender?.Email ?? "N/A",
                    LenderProfilePicture = debt.Lender?.ProfilePicture ?? "N/A",
                    BorrowerId = debt.Borrower?.Id ?? 0,
                    BorrowerName = debt.Borrower?.UserName ?? "N/A",
                    BorrowerEmail = debt.Borrower?.Email ?? "N/A",
                    BorrowerProfilePicture = debt.Borrower?.ProfilePicture ?? "N/A",
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
                || string.IsNullOrEmpty(request.BorrowerEmail)
            )
            {
                _logger.LogWarning(
                    "Invalid debt offer request received: {Request}.",
                    request
                );
                return BadRequest("Invalid debt offer request.");
            }

            int userId = GetAuthenticatedId();

            var borrower = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.BorrowerEmail);
            if (borrower == null)
            {
                return BadRequest("Borrower with the specified email not found.");
            }

            if (borrower.Id == userId)
            {
                return BadRequest("Lender and borrower cannot be the same person.");
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
            CreateDebtOfferResult result = await _secureDebtService.CreateDebtOfferAsync(
                lender,
                borrower,
                request.Amount,
                request.CurrencyCode,
                request.DueDateUtc,
                request.Description
            );
            if (!result.Success)
            {
                _logger.LogError(
                    result.Message
                );
                return StatusCode(500, result.Message);
            }
            _logger.LogInformation(
                result.Message
            );
            return Ok(new { Success = true, Message = result.Message, DebtId = result.DebtId });
        }

        // Borç teklifini kabul etme metodu.
        [HttpPost("respond-to-offer/{debtId}")]
        public async Task<IActionResult> RespondToOfferAsync(int debtId, [FromBody] RespondToOfferRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("RespondToOfferRequestDto is null.");
                return BadRequest("Invalid request data.");
            }

            bool decision = request.Accepted;

            int userId = GetAuthenticatedId();

            try
            {
                var debt = await _context
                    .Debts.Include(d => d.Borrower)
                    .FirstOrDefaultAsync(d => d.Id == debtId);

                if (debt == null)
                {
                    _logger.LogInformation("Debt offer with ID {DebtId} not found.", debtId);
                    return NotFound("Debt offer not found.");
                }

                if (debt.BorrowerId != userId)
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
                        "Debt offer with ID {DebtId} is not in a state that can be accepted. Current status: {Status}",
                        debtId,
                        debt.Status
                    );
                    return BadRequest("Debt offer is not in a state that can be accepted.");
                }

                if (decision)
                {
                    debt.Status = DebtStatusType.AcceptedPendingVideoUpload;
                    debt.BorrowerApprovalAtUtc = DateTime.UtcNow;
                    debt.UpdatedAtUtc = DateTime.UtcNow;
                    _logger.LogInformation("Debt offer {DebtId} accepted by user {UserId}. Now pending video upload.", debtId, userId);
                }
                else
                {
                    debt.Status = DebtStatusType.RejectedByBorrower;
                    debt.UpdatedAtUtc = DateTime.UtcNow;
                    _logger.LogInformation("Debt offer {DebtId} rejected by user {UserId}.", debtId, userId);
                }

                _context.Debts.Update(debt);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing debt offer decision for debt ID {DebtId}.", debtId);
                return StatusCode(500, "Internal server error while processing the debt offer.");
            }
        }

        // Borcun vadesi geçtiğinde alacaklının borcu "Defaulted" olarak işaretlemesi için.
        [HttpPost("mark-as-defaulted/{debtId}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> MarkAsDefaulted(int debtId)
        {
            int userId = GetAuthenticatedId();
            var debt = await _context.Debts
                .Include(d => d.Lender)
                .FirstOrDefaultAsync(d => d.Id == debtId);

            if (debt == null)
            {
                return NotFound("Debt not found.");
            }

            if (debt.LenderId != userId)
            {
                return Forbid("You are not authorized to perform this action on this debt.");
            }

            if (debt.Status != DebtStatusType.Active)
            {
                return BadRequest($"This action can only be performed on 'Active' debts. Current status is '{debt.Status}'.");
            }

            if (debt.DueDateUtc > DateTime.UtcNow)
            {
                return BadRequest($"The due date ({debt.DueDateUtc:yyyy-MM-dd}) for this debt has not passed yet.");
            }

            debt.Status = DebtStatusType.Defaulted;
            debt.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Debt with ID {DebtId} has been marked as 'Defaulted' by lender {UserId}.", debtId, userId);

            return Ok(new { Success = true, Message = "Debt has been successfully marked as defaulted. You can now access the video evidence." });
        }
    }
}

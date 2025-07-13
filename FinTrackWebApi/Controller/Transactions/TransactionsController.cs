using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.TransactionDtos;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Controller.Transactions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class TransactionsController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(MyDataContext context, ILogger<TransactionsController> logger)
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var transactions = await _context
                    .Transactions.AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        Category = t.Category,
                        Account = t.Account,
                        Amount = t.Amount,
                        TransactionDateUtc = t.TransactionDateUtc,
                        Description = t.Description,
                        CreatedAtUtc = t.CreatedAtUtc,
                        UpdatedAtUtc = t.UpdatedAtUtc
                    })
                    .ToListAsync();

                if (transactions == null || transactions.Count == 0)
                {
                    _logger.LogInformation(
                        "No transactions found for user {UserId}",
                        userId
                    );
                    return NotFound($"No transactions found for user {userId}.");
                }

                _logger.LogInformation(
                    "Retrieved {TransactionCount} transactions for user {UserId}",
                    transactions.Count,
                    userId
                );

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving transactions for user {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving the transactions.");
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int Id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var transaction = await _context
                    .Transactions.AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId && t.Id == Id)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        Category = t.Category,
                        Account = t.Account,
                        Amount = t.Amount,
                        TransactionDateUtc = t.TransactionDateUtc,
                        Description = t.Description,
                        CreatedAtUtc = t.CreatedAtUtc,
                        UpdatedAtUtc = t.UpdatedAtUtc
                    })
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    _logger.LogWarning(
                        "Transaction with ID {TransactionId} not found for user {UserId}",
                        Id,
                        userId
                    );
                    return NotFound(
                        $"Transaction with ID {Id} not found for user {userId}."
                    );
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving transaction with ID {TransactionId} for user {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving the transaction.");
            }
        }

        [HttpGet("category-type/{type}")]
        public async Task<ActionResult<TransactionDto>> GetTransactionsByType(string type)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                if (!Enum.TryParse(type, true, out TransactionCategoryType categoryType))
                {
                    _logger.LogWarning(
                        "Invalid category type '{CategoryType}' provided by user {UserId}",
                        type,
                        userId
                    );
                    return BadRequest($"Invalid category type '{type}'.");
                }

                var transactions = await _context
                    .Transactions.AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId && t.Type == categoryType)
                    .ToListAsync();

                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound(
                        $"No transactions found for category type '{type}' for user {userId}."
                    );
                }

                _logger.LogInformation(
                    "Retrieved {TransactionCount} transactions for category type '{CategoryType}' for user {UserId}",
                    transactions.Count,
                    type,
                    userId
                );

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving transactions for category type '{CategoryType}' for user {UserId}",
                    type,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving the transactions.");
            }
        }

        [HttpGet("category-name/{category}")]
        public async Task<IActionResult> GetTransactionsByCategoryName(string category)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var transactions = await _context
                    .Transactions.AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.UserId == userId && t.Category.Name == category)
                    .ToListAsync();

                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound(
                        $"No transactions found for category name '{category}' for user {userId}."
                    );
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving transactions for category name '{CategoryName}' for user {UserId}",
                    category,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving the transactions.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(
            [FromBody] TransactionCreateDto transactionDto
        )
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for transaction creation: {ModelStateErrors}",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                );
                return BadRequest(ModelState);
            }

            int userId = GetAuthenticatedUserId();

            var category = await _context.Categories.FirstOrDefaultAsync(c =>
                c.Id == transactionDto.CategoryId && c.UserId == userId
            );
            if (category == null)
            {
                _logger.LogWarning(
                    "Category with ID {CategoryId} not found for user {UserId}",
                    transactionDto.CategoryId,
                    userId
                );
                return BadRequest(
                    $"Category with ID {transactionDto.CategoryId} not found for this user."
                );
            }

            var transaction = new TransactionModel
            {
                UserId = userId,
                CategoryId = transactionDto.CategoryId,
                AccountId = transactionDto.AccountId,
                Amount = transactionDto.Amount,
                TransactionDateUtc = DateTime.SpecifyKind(
                    transactionDto.TransactionDateUtc,
                    DateTimeKind.Utc
                ),
                Description = transactionDto.Description,
                CreatedAtUtc = DateTime.UtcNow,
            };

            _context.Transactions.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Transaction created with ID {TransactionId} for user {UserId}",
                    transaction.Id,
                    userId
                );

                return Ok(true);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving new transaction for user {UserId}", userId);
                return StatusCode(500, "An error occurred while saving the transaction.");
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateTransaction(
            int Id,
            [FromBody] TransactionUpdateDto transactionDto
        )
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state for transaction update: {ModelStateErrors}",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                );
                return BadRequest(ModelState);
            }

            int userId = GetAuthenticatedUserId();

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t =>
                t.Id == Id && t.UserId == userId
            );

            if (transaction == null)
            {
                _logger.LogWarning(
                    "Transaction with ID {TransactionId} not found for user {UserId}",
                    Id,
                    userId
                );
                return NotFound(
                    $"Transaction with ID {Id} not found for user {userId}."
                );
            }

            transaction.CategoryId = transactionDto.CategoryId;
            transaction.AccountId = transactionDto.AccountId;
            transaction.Amount = transactionDto.Amount;
            transaction.TransactionDateUtc = DateTime.SpecifyKind(
                transactionDto.TransactionDateUtc,
                DateTimeKind.Utc
            );
            transaction.Description = transactionDto.Description;
            transaction.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Transaction with ID {TransactionId} updated for user {UserId}",
                    transaction.Id,
                    userId
                );
                return Ok(true);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating transaction with ID {TransactionId} for user {UserId}",
                    transaction.Id,
                    userId
                );
                return StatusCode(500, "An error occurred while updating the transaction.");
            }
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteTransaction(int Id)
        {
            int userId = GetAuthenticatedUserId();

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t =>
                t.Id == Id && t.UserId == userId
            );
            if (transaction == null)
            {
                _logger.LogWarning(
                    "Transaction with ID {TransactionId} not found for user {UserId}",
                    Id,
                    userId
                );
                return NotFound(
                    $"Transaction with ID {Id} not found for user {userId}."
                );
            }
            _context.Transactions.Remove(transaction);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Transaction with ID {TransactionId} deleted for user {UserId}",
                    transaction.Id,
                    userId
                );

                _logger.LogInformation(
                    "Transaction with ID {TransactionId} deleted successfully for user {UserId}",
                    transaction.Id,
                    userId
                );

                return Ok(true);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting transaction with ID {TransactionId} for user {UserId}",
                    transaction.Id,
                    userId
                );
                return StatusCode(500, "An error occurred while deleting the transaction.");
            }
        }
    }
}

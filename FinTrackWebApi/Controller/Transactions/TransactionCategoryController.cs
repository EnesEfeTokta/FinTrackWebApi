using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Dtos.TransactionDtos;

namespace FinTrackWebApi.Controller.Transactions
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin,User")]
    public class TransactionCategoryController : ControllerBase
    {
        private readonly ILogger<TransactionCategoryController> _logger;
        private readonly MyDataContext _context;

        public TransactionCategoryController(ILogger<TransactionCategoryController> logger, MyDataContext context)
        {
            _logger = logger;
            _context = context;
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
        public async Task<IActionResult> GetTransactionCategories()
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var transactionCategories = await _context.TransactionCategories
                    .Where(tc => tc.UserId == userId)
                    .Select(tc => new TransactionCategoriesDto
                    {
                        Id = tc.Id,
                        Name = tc.Name,
                        Type = tc.Type,
                        CreatedAt = tc.CreatedAt,
                        UpdatedAt = tc.UpdatedAt
                    })
                    .ToListAsync();

                if (transactionCategories == null || !transactionCategories.Any())
                {
                    _logger.LogInformation("No transaction categories found for user {UserId}", userId);
                    return NotFound("No transaction categories found for the user.");
                }

                _logger.LogInformation("Retrieved {Count} transaction categories for user {UserId}", transactionCategories.Count, userId);
                return Ok(transactionCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction categories for user {UserId}", userId);
                return StatusCode(500, "Internal server error while retrieving transaction categories.");
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetTransactionCategory(int Id)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var transactionCategory = await _context.TransactionCategories
                    .Where(tc => tc.Id == Id && tc.UserId == userId)
                    .Select(tc => new TransactionCategoriesDto
                    {
                        Id = tc.Id,
                        Name = tc.Name,
                        Type = tc.Type,
                        CreatedAt = tc.CreatedAt,
                        UpdatedAt = tc.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (transactionCategory == null)
                {
                    _logger.LogInformation("Transaction category with ID {Id} not found for user {UserId}", Id, userId);
                    return NotFound($"Transaction category with ID {Id} not found for the user.");
                }

                _logger.LogInformation("Retrieved transaction category with ID {Id} for user {UserId}", Id, userId);
                return Ok(transactionCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction category with ID {Id} for user {UserId}", Id, userId);
                return StatusCode(500, "Internal server error while retrieving transaction category.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransactionCategory([FromBody] TransactionCategoriesDto transactionCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for transaction category creation.");
                return BadRequest(ModelState);
            }

            int userId = GetAuthenticatedUserId();

            try
            {
                var transactionCategory = new Models.Tranaction.TransactionCategoryModel
                {
                    Name = transactionCategoryDto.Name,
                    Type = transactionCategoryDto.Type,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TransactionCategories.Add(transactionCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new transaction category with ID {Id} for user {UserId}", transactionCategory.Id, userId);
                return CreatedAtAction(nameof(GetTransactionCategory), new { Id = transactionCategory.Id }, transactionCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction category for user {UserId}", userId);
                return StatusCode(500, "Internal server error while creating transaction category.");
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateTransactionCategory(int Id, [FromBody] TransactionCategoriesDto transactionCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for transaction category update.");
                return BadRequest(ModelState);
            }

            int userId = GetAuthenticatedUserId();

            try
            {
                var transactionCategory = await _context.TransactionCategories
                    .FirstOrDefaultAsync(tc => tc.Id == Id && tc.UserId == userId);
                if (transactionCategory == null)
                {
                    _logger.LogInformation("Transaction category with ID {Id} not found for user {UserId}", Id, userId);
                    return NotFound($"Transaction category with ID {Id} not found for the user.");
                }

                transactionCategory.Name = transactionCategoryDto.Name;
                transactionCategory.Type = transactionCategoryDto.Type;
                transactionCategory.UpdatedAt = DateTime.UtcNow;

                _context.TransactionCategories.Update(transactionCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated transaction category with ID {Id} for user {UserId}", Id, userId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction category with ID {Id} for user {UserId}", Id, userId);
                return StatusCode(500, "Internal server error while updating transaction category.");
            }
        }

        [HttpDelete("{Id}")]
        public IActionResult DeleteTransactionCategory(int Id)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var transactionCategory = _context.TransactionCategories
                    .FirstOrDefault(tc => tc.Id == Id && tc.UserId == userId);
                if (transactionCategory == null)
                {
                    _logger.LogInformation("Transaction category with ID {Id} not found for user {UserId}", Id, userId);
                    return NotFound($"Transaction category with ID {Id} not found for the user.");
                }

                _context.TransactionCategories.Remove(transactionCategory);
                _context.SaveChanges();

                _logger.LogInformation("Deleted transaction category with ID {Id} for user {UserId}", Id, userId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction category with ID {Id} for user {UserId}", Id, userId);
                return StatusCode(500, "Internal server error while deleting transaction category.");
            }
        }
    }
}

using System.Security.Claims;
using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetCategoryController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<BudgetCategoryController> _logger;

        public BudgetCategoryController(
            MyDataContext context,
            ILogger<BudgetCategoryController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError(
                    "Authenticated user ID claim (NameIdentifier) not found or invalid in token for user {UserName}.",
                    User.Identity?.Name ?? "Unknown"
                );
                throw new UnauthorizedAccessException(
                    "User ID cannot be determined from the token."
                );
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetCategories()
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budgetCategories = await _context
                    .BudgetCategories.AsNoTracking()
                    .Where(bc => bc.Budget.UserId == authenticatedUserId)
                    .ToListAsync();

                if (budgetCategories == null || !budgetCategories.Any())
                {
                    _logger.LogWarning(
                        "No budget categories found for user ID: {UserId}",
                        authenticatedUserId
                    );
                    return NotFound("No budget categories found.");
                }

                _logger.LogInformation(
                    "Successfully retrieved budget categories for user ID: {UserId}",
                    authenticatedUserId
                );
                return Ok(budgetCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving budget categories for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving budget categories.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudgetCategory(int id)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budgetCategory = await _context
                    .BudgetCategories.AsNoTracking()
                    .FirstOrDefaultAsync(bc =>
                        bc.Id == id && bc.Budget.UserId == authenticatedUserId
                    );

                if (budgetCategory == null)
                {
                    _logger.LogWarning(
                        "Budget category with ID {BudgetCategoryId} not found for user ID: {UserId}",
                        id,
                        authenticatedUserId
                    );
                    return NotFound($"Budget category with ID {id} not found.");
                }

                _logger.LogInformation(
                    "Successfully retrieved budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    authenticatedUserId
                );
                return Ok(budgetCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving budget category.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudgetCategory(
            [FromBody] BudgetCategoryCreateDto budgetCategoryDto
        )
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budget = await _context
                    .Budgets.AsNoTracking()
                    .FirstOrDefaultAsync(b =>
                        b.Id == budgetCategoryDto.BudgetId && b.UserId == authenticatedUserId
                    );

                if (budget == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        budgetCategoryDto.BudgetId,
                        authenticatedUserId
                    );
                    return NotFound($"Budget with ID {budgetCategoryDto.BudgetId} not found.");
                }

                var budgetCategory = new BudgetCategoryModel
                {
                    BudgetId = budgetCategoryDto.BudgetId,
                    CategoryId = budgetCategoryDto.CategoryId,
                    AllocatedAmount = budgetCategoryDto.AllocatedAmount,
                };

                await _context.BudgetCategories.AddAsync(budgetCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully created budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    budgetCategory.Id,
                    authenticatedUserId
                );
                return CreatedAtAction(
                    nameof(GetBudgetCategory),
                    new { id = budgetCategory.Id },
                    budgetCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating budget category for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while creating budget category.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudgetCategory(
            int id,
            [FromBody] BudgetCategoryUpdateDto budgetCategoryDto
        )
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budgetCategory = await _context.BudgetCategories.FirstOrDefaultAsync(bc =>
                    bc.Id == id && bc.Budget.UserId == authenticatedUserId
                );

                if (budgetCategory == null)
                {
                    _logger.LogWarning(
                        "Budget category with ID {BudgetCategoryId} not found for user ID: {UserId}",
                        id,
                        authenticatedUserId
                    );
                    return NotFound($"Budget category with ID {id} not found.");
                }

                budgetCategory.AllocatedAmount = budgetCategoryDto.AllocatedAmount;
                _context.BudgetCategories.Update(budgetCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully updated budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    authenticatedUserId
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while updating budget category.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudgetCategory(int id)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budgetCategory = await _context.BudgetCategories.FirstOrDefaultAsync(bc =>
                    bc.Id == id && bc.Budget.UserId == authenticatedUserId
                );

                if (budgetCategory == null)
                {
                    _logger.LogWarning(
                        "Budget category with ID {BudgetCategoryId} not found for user ID: {UserId}",
                        id,
                        authenticatedUserId
                    );
                    return NotFound($"Budget category with ID {id} not found.");
                }

                _context.BudgetCategories.Remove(budgetCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully deleted budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    authenticatedUserId
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting budget category with ID {BudgetCategoryId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while deleting budget category.");
            }
        }
    }
}

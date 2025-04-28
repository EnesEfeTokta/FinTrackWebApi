using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<BudgetsController> _logger;

        public BudgetsController(MyDataContext context, ILogger<BudgetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid in token for user {UserName}.", User.Identity?.Name ?? "Unknown");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }

        [HttpGet("budgets")]
        public async Task<IActionResult> GetBudgets()
        {
            int authenticatedUserId = GetAuthenticatedUserId();

            var budgets = await _context.Budgets
                .AsNoTracking()
                .Where(b => b.UserId == authenticatedUserId)
                .ToListAsync();

            if (budgets == null || !budgets.Any())
            {
                _logger.LogWarning("No budgets found for user ID: {UserId}", authenticatedUserId);
                return NotFound("No budgets found.");
            }

            _logger.LogInformation("Successfully retrieved budgets for user ID: {UserId}", authenticatedUserId);
            return Ok(budgets);
        }

        [HttpGet("get-budget/{id}")]
        public async Task<IActionResult> GetBudget(int id)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budget = await _context.Budgets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == authenticatedUserId);

                if (budget == null)
                {
                    _logger.LogWarning("Budget with ID {BudgetId} not found for user ID: {UserId}", id, authenticatedUserId);
                    return NotFound("Budget not found.");
                }

                _logger.LogInformation("Successfully retrieved budget with ID {BudgetId} for user ID: {UserId}", id, authenticatedUserId);
                return Ok(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget with ID {BudgetId} for user ID: {UserId}", id, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while retrieving budget.");
            }
        }

        [HttpPost("create-budget")]
        public async Task<IActionResult> CreateBudget([FromBody] BudgetDto budgetDto)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();
                var budget = new BudgetModel
                {
                    UserId = authenticatedUserId,
                    Name = budgetDto.Name,
                    Description = budgetDto.Description,
                    StartDate = DateTime.SpecifyKind(budgetDto.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(budgetDto.EndDate, DateTimeKind.Utc),
                    IsActive = budgetDto.IsActive,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = null
                };

                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created budget for user ID: {UserId}", authenticatedUserId);
                return CreatedAtAction(nameof(GetBudgets), new { id = budget.BudgetId }, budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget for user ID: {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while creating budget.");
            }
        }

        [HttpPut("update-budget/{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] BudgetDto budgetDto)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budget = await _context.Budgets.FindAsync(id);
                if (budget == null || budget.UserId != authenticatedUserId)
                {
                    _logger.LogWarning("Budget with ID {BudgetId} not found for user ID: {UserId}", id, authenticatedUserId);
                    return NotFound("Budget not found.");
                }

                budget.Name = budgetDto.Name;
                budget.Description = budgetDto.Description;
                budget.StartDate = budgetDto.StartDate;
                budget.EndDate = budgetDto.EndDate;
                budget.IsActive = budgetDto.IsActive;
                budget.UpdatedAtUtc = DateTime.UtcNow;

                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated budget with ID {BudgetId} for user ID: {UserId}", id, authenticatedUserId);
                return Ok(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget with ID {BudgetId} for user ID: {UserId}", id, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while updating budget.");
            }
        }


        [HttpDelete("delete-budget/{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budget = await _context.Budgets.FindAsync(id);
                if (budget == null || budget.UserId != authenticatedUserId)
                {
                    _logger.LogWarning("Budget with ID {BudgetId} not found for user ID: {UserId}", id, authenticatedUserId);
                    return NotFound("Budget not found.");
                }

                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted budget with ID {BudgetId} for user ID: {UserId}", id, authenticatedUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget with ID {BudgetId} for user ID: {UserId}", id, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while deleting budget.");
            }
        }

        [HttpGet("get-budget-categories/{id}")]
        public async Task<IActionResult> GetBudgetCategories(int id)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == authenticatedUserId);

                if (budget == null)
                {
                    _logger.LogWarning("Budget with ID {BudgetId} not found for user ID: {UserId}", id, authenticatedUserId);
                    return NotFound("Budget not found.");
                }

                _logger.LogInformation("Successfully retrieved budget categories for budget ID {BudgetId} and user ID: {UserId}", id, authenticatedUserId);
                return Ok(budget.BudgetCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget categories for budget ID {BudgetId} and user ID: {UserId}", id, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while retrieving budget categories.");
            }
        }
    }
}

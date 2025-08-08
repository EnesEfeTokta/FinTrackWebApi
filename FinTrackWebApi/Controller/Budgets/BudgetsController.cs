using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.BudgetDtos;
using FinTrackWebApi.Models.Budget;
using FinTrackWebApi.Models.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Budgets
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
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
        public async Task<IActionResult> GetBudgets()
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var budgets = await _context.Budgets
                    .Where(b => b.UserId == userId)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Category = b.Category.Name,
                        AllocatedAmount = b.AllocatedAmount,
                        ReachedAmount = b.ReachedAmount,
                        Currency = b.Currency,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        IsActive = b.IsActive,
                        CreatedAtUtc = b.CreatedAtUtc,
                        UpdatedAtUtc = b.UpdatedAtUtc
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!budgets.Any())
                {
                    _logger.LogWarning(
                        "No budgets found for user ID: {UserId}",
                        userId
                    );
                    return Ok(new List<BudgetDto>());
                }

                _logger.LogInformation(
                    "Successfully retrieved budgets for user ID: {UserId}",
                    userId
                );

                return Ok(budgets);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving budgets for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving budgets.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(int id)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var budget = await _context.Budgets.Where(b => b.Id == id && b.UserId == userId)
                    .Include(b => b.Category)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Category = b.Category.Name,
                        AllocatedAmount = b.AllocatedAmount,
                        ReachedAmount = b.ReachedAmount,
                        Currency = b.Currency,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        IsActive = b.IsActive,
                        CreatedAtUtc = b.CreatedAtUtc,
                        UpdatedAtUtc = b.UpdatedAtUtc
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (budget == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        id,
                        userId
                    );
                    return NotFound("Budget not found.");
                }

                _logger.LogInformation(
                    "Successfully retrieved budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    userId
                );

                return Ok(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving budget.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] BudgetCreateDto budgetDto)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == budgetDto.Category && c.UserId == userId);
                if (category == null)
                {
                    category = new CategoryModel
                    {
                        UserId = userId,
                        Name = budgetDto.Category
                    };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        "Creating new category '{CategoryName}' for user ID: {UserId}",
                        budgetDto.Category,
                        userId
                    );
                }

                var newBudget = new BudgetModel
                {
                    UserId = userId,
                    CategoryId = category.Id,
                    Name = budgetDto.Name,
                    Description = budgetDto.Description,
                    AllocatedAmount = budgetDto.AllocatedAmount,
                    ReachedAmount = budgetDto.ReachedAmount,
                    Currency = budgetDto.Currency,
                    StartDate = budgetDto.StartDate,
                    EndDate = budgetDto.EndDate,
                    IsActive = budgetDto.IsActive,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Budgets.Add(newBudget);
                await _context.SaveChangesAsync();

                var resultDto = await GetBudget(newBudget.Id) as OkObjectResult;

                _logger.LogInformation(
                    "Successfully created budget for user ID: {UserId}",
                    userId
                );

                return CreatedAtAction(nameof(GetBudget), new { id = newBudget.Id }, resultDto?.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating budget for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while creating budget.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] BudgetUpdateDto budgetDto)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var budgetToUpdate = await _context.Budgets
                    .FirstOrDefaultAsync(bc => bc.Id == id && bc.UserId == userId);
                if (budgetToUpdate == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        id,
                        userId
                    );
                    return NotFound("Budget not found.");
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == budgetDto.Category && c.UserId == userId);
                if (category == null)
                {
                    category = new CategoryModel
                    {
                        UserId = userId,
                        Name = budgetDto.Category
                    };
                    _context.Categories.Add(category);
                }

                budgetToUpdate.Name = budgetDto.Name;
                budgetToUpdate.Description = budgetDto.Description;
                budgetToUpdate.AllocatedAmount = budgetDto.AllocatedAmount;
                budgetToUpdate.ReachedAmount = budgetDto.ReachedAmount;
                budgetToUpdate.Currency = budgetDto.Currency;
                budgetToUpdate.StartDate = budgetDto.StartDate;
                budgetToUpdate.EndDate = budgetDto.EndDate;
                budgetToUpdate.IsActive = budgetDto.IsActive;
                budgetToUpdate.Category = category;
                budgetToUpdate.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new BudgetDto
                {
                    Id = budgetToUpdate.Id,
                    Name = budgetToUpdate.Name,
                    Description = budgetToUpdate.Description,
                    Category = budgetToUpdate.Category.Name,
                    AllocatedAmount = budgetToUpdate.AllocatedAmount,
                    ReachedAmount = budgetToUpdate.ReachedAmount,
                    Currency = budgetToUpdate.Currency,
                    StartDate = budgetToUpdate.StartDate,
                    EndDate = budgetToUpdate.EndDate,
                    IsActive = budgetToUpdate.IsActive,
                    CreatedAtUtc = budgetToUpdate.CreatedAtUtc,
                    UpdatedAtUtc = budgetToUpdate.UpdatedAtUtc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while updating budget.");
            }
        }

        [HttpPut("Update-Reached-Amount")]
        public async Task<IActionResult> UpdateReachedAmount([FromBody] BudgetUpdateReachedAmountDto amountDto)
        {
            if (amountDto == null || amountDto.BudgetId <= 0)
            {
                return BadRequest("Invalid budget data provided.");
            }

            int userId = GetAuthenticatedUserId();
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(bc => bc.Id == amountDto.BudgetId && bc.UserId == userId);

                if (budget == null)
                {
                    _logger.LogWarning(
                        "Update failed. Budget with ID {BudgetId} not found for user ID: {UserId}",
                        amountDto.BudgetId,
                        userId
                    );
                    return NotFound($"Budget with ID {amountDto.BudgetId} not found.");
                }

                budget.ReachedAmount = amountDto.ReachedAmount;
                budget.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reached Amount has been updated for budget ID: {BudgetId}",
                    budget.Id
                );

                return Ok(new BudgetDto
                {
                    Id = budget.Id,
                    Name = budget.Name,
                    Description = budget.Description,
                    Category = budget.Category?.Name ?? "Other",
                    AllocatedAmount = budget.AllocatedAmount,
                    ReachedAmount = budget.ReachedAmount,
                    Currency = budget.Currency,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    IsActive = budget.IsActive,
                    CreatedAtUtc = budget.CreatedAtUtc,
                    UpdatedAtUtc = budget.UpdatedAtUtc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating reached amount for budget with ID {BudgetId} for user ID: {UserId}",
                    amountDto.BudgetId,
                    userId
                );
                return StatusCode(500, "Internal server error while updating budget amount.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var budgetToDelete = await _context.Budgets
                    .FirstOrDefaultAsync(bc => bc.Id == id && bc.UserId == userId);
                if (budgetToDelete == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        id,
                        userId
                    );
                    return NotFound("Budget not found.");
                }

                _context.Budgets.Remove(budgetToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully deleted budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    userId
                );
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while deleting budget.");
            }
        }
    }
}

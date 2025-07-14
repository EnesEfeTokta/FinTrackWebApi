using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.BudgetDtos;
using FinTrackWebApi.Models;
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
            try
            {
                int userId = GetAuthenticatedUserId();

                var bc = await _context
                    .BudgetCategories
                    .Where(b => b.Budget.UserId == userId && b.Category.UserId == userId)
                    .Select(b => new
                    {
                        b.Budget.Id,
                        b.Budget.Name,
                        b.Budget.Description,
                        Category = b.Category.Name,
                        b.AllocatedAmount,
                        b.Currency,
                        b.Budget.StartDate,
                        b.Budget.EndDate,
                        b.Budget.IsActive,
                        b.Budget.CreatedAtUtc,
                        b.Budget.UpdatedAtUtc
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (bc == null || !bc.Any())
                {
                    _logger.LogWarning(
                        "No budgets found for user ID: {UserId}",
                        userId
                    );
                    return NotFound("No budgets found.");
                }
                _logger.LogInformation(
                    "Successfully retrieved budgets for user ID: {UserId}",
                    userId
                );

                var budgetDtos = new List<BudgetDto>();
                foreach (var budget in bc)
                {
                    budgetDtos.Add(new BudgetDto
                    {
                        Id = budget.Id,
                        Name = budget.Name,
                        Description = budget.Description,
                        Category = budget.Category,
                        AllocatedAmount = budget.AllocatedAmount,
                        Currency = budget.Currency,
                        StartDate = budget.StartDate,
                        EndDate = budget.EndDate,
                        IsActive = budget.IsActive,
                        CreatedAtUtc = budget.CreatedAtUtc,
                        UpdatedAtUtc = budget.UpdatedAtUtc
                    });
                }

                return Ok(budgetDtos);
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
            try
            {
                int userId = GetAuthenticatedUserId();

                var bc = await _context
                    .BudgetCategories.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Budget.UserId == userId && b.Category.UserId == userId);

                if (bc == null)
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

                var budgetDto = new BudgetDto
                {
                    Id = bc.Id,
                    Name = bc.Budget.Name,
                    Description = bc.Budget.Description,
                    Category = bc.Category.Name,
                    AllocatedAmount = bc.AllocatedAmount,
                    Currency = bc.Currency,
                    StartDate = bc.Budget.StartDate,
                    EndDate = bc.Budget.EndDate,
                    IsActive = bc.Budget.IsActive,
                    CreatedAtUtc = bc.CreatedAtUtc,
                    UpdatedAtUtc = bc.UpdatedAtUtc
                };

                return Ok(budgetDto);
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
                var newBudget = new BudgetModel
                {
                    UserId = userId,
                    Name = budgetDto.Name,
                    Description = budgetDto.Description,
                    StartDate = DateTime.UtcNow,
                    IsActive = budgetDto.IsActive,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Budgets.Add(newBudget);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully created budget for user ID: {UserId}",
                    userId
                );
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

            try
            {
                var category = await _context.Categories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name == budgetDto.Category && c.UserId == userId);
                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryName} not found for user ID: {UserId}",
                        budgetDto.Category,
                        userId
                    );

                    var newCategory = new CategoryModel
                    {
                        UserId = userId,
                        Name = budgetDto.Category,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Created new category {CategoryName} for user ID: {UserId}",
                        budgetDto.Category,
                        userId
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "Category {CategoryName} already exists for user ID: {UserId}",
                        budgetDto.Category,
                        userId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking or creating category {CategoryName} for user ID: {UserId}",
                    budgetDto.Category,
                    userId
                );
                return StatusCode(500, "Internal server error while checking or creating category.");
            }

            try
            {
                var budget = await _context.Budgets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Name == budgetDto.Name && b.UserId == userId);
                if (budget == null)
                {
                    _logger.LogWarning(
                        "Budget {BudgetName} not found for user ID: {UserId}",
                        budgetDto.Name,
                        userId
                    );
                }
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == budgetDto.Category && c.UserId == userId);
                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryName} not found for user ID: {UserId}",
                        budgetDto.Category,
                        userId
                    );
                }

                var budgetCategory = new BudgetCategoryModel
                {
                    BudgetId = budget.Id,
                    CategoryId = category.Id,
                    AllocatedAmount = budgetDto.AllocatedAmount,
                    Currency = budgetDto.Currency,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.BudgetCategories.Add(budgetCategory);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Successfully created budget category for budget ID: {BudgetId} and category ID: {CategoryId}",
                    budget.Id,
                    category.Id
                );
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating budget category for user ID: {UserId}",
                    userId
                );
                return StatusCode(500, "Internal server error while creating budget category.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] BudgetUpdateDto budgetDto)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var bc = await _context.BudgetCategories.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);
                if (bc == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        id,
                        userId
                    );
                    return NotFound("Budget not found.");
                }

                bc.Budget.Name = budgetDto.Name;
                bc.Budget.Description = budgetDto.Description;
                bc.Category.Name = budgetDto.Category;
                bc.AllocatedAmount = budgetDto.AllocatedAmount;
                bc.Currency = budgetDto.Currency;
                bc.Budget.IsActive = budgetDto.IsActive;
                bc.Budget.StartDate = budgetDto.StartDate;
                bc.Budget.EndDate = budgetDto.EndDate;
                bc.Budget.IsActive = budgetDto.IsActive;
                bc.UpdatedAtUtc = DateTime.UtcNow;

                _context.BudgetCategories.Update(bc);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully updated budget with ID {BudgetId} for user ID: {UserId}",
                    id,
                    userId
                );
                return Ok(bc);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var bc = await _context.BudgetCategories.AsNoTracking()
                    .FirstOrDefaultAsync(bc => bc.Id == id);
                if (bc == null)
                {
                    _logger.LogWarning(
                        "Budget with ID {BudgetId} not found for user ID: {UserId}",
                        id,
                        userId
                    );
                    return NotFound("Budget not found.");
                }

                _context.BudgetCategories.Remove(bc);
                await _context.SaveChangesAsync();

                _context.Budgets.Remove(bc.Budget);
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

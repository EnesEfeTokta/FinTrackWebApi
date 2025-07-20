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
            try
            {
                int userId = GetAuthenticatedUserId();

                var budgets = await _context.Budgets
                    .Where(b => b.UserId == userId)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Category = b.Category.Name,
                        AllocatedAmount = b.AllocatedAmount,
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
            try
            {
                int userId = GetAuthenticatedUserId();

                var budget = await _context.Budgets.Where(b => b.Id == id && b.UserId == userId)
                    .Include(b => b.Category)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Category = b.Category.Name,
                        AllocatedAmount = b.AllocatedAmount,
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
            try
            {
                int userId = GetAuthenticatedUserId();

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

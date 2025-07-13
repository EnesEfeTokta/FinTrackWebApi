using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.CategoriesDtos;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Categories
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(MyDataContext context, ILogger<CategoriesController> logger)
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
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var categoriesFromDb = await _context
                    .Categories.AsNoTracking()
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (categoriesFromDb == null || categoriesFromDb.Count == 0)
                {
                    _logger.LogWarning(
                        "No categories found for user ID: {UserId}",
                        userId
                    );
                    return NotFound("No categories found.");
                }

                var categories = new List<CategoryDto>();
                foreach (var category in categoriesFromDb)
                {
                    categories.Add(new CategoryDto
                    {
                        Id = category.Id,
                        Name = category.Name,
                        CreatedAtUtc = category.CreatedAtUtc,
                        UpdatedAtUtc = category.UpdatedAtUtc
                    });
                }

                _logger.LogInformation(
                    "Successfully retrieved categories for user ID: {UserId}",
                    userId
                );
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving categories for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving categories.");
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int Id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var category = await _context
                    .Categories.AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId && c.Id == Id
                    );

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category with ID {CategoryId} not found for user ID: {UserId}",
                        Id,
                        userId
                    );
                    return NotFound($"Category with ID {Id} not found.");
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAtUtc = category.CreatedAtUtc,
                    UpdatedAtUtc = category.UpdatedAtUtc
                };

                _logger.LogInformation(
                    "Successfully retrieved category with ID {CategoryId} for user ID: {UserId}",
                    Id,
                    userId
                );
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving category with ID {CategoryId} for user ID: {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while retrieving the category.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryDto)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var newCategory = new CategoryModel
                {
                    UserId = userId,
                    Name = categoryDto.Name,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully created category for user ID: {UserId}",
                    userId
                );
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating category for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while creating the category.");
            }
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(
            int categoryId,
            [FromBody] CategoryUpdateDto categoryDto
        )
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c =>
                    c.UserId == userId && c.Id == categoryId
                );

                if (existingCategory == null)
                {
                    _logger.LogWarning(
                        "Category with ID {CategoryId} not found for user ID: {UserId}",
                        categoryId,
                        userId
                    );
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                existingCategory.Name = categoryDto.Name;
                existingCategory.UpdatedAtUtc = DateTime.UtcNow;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully updated category with ID {CategoryId} for user ID: {UserId}",
                    categoryId,
                    userId
                );
                return Ok(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating category with ID {CategoryId} for user ID: {UserId}",
                    categoryId,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while updating the category.");
            }
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c =>
                    c.UserId == userId && c.Id == categoryId
                );

                if (existingCategory == null)
                {
                    _logger.LogWarning(
                        "Category with ID {CategoryId} not found for user ID: {UserId}",
                        categoryId,
                        userId
                    );
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                _context.Categories.Remove(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully deleted category with ID {CategoryId} for user ID: {UserId}",
                    categoryId,
                    userId
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting category with ID {CategoryId} for user ID: {UserId}",
                    categoryId,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while deleting the category.");
            }
        }
    }
}

using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                _logger.LogError("Authenticated user ID claim (NameIdentifier) not found or invalid in token for user {UserName}.", User.Identity?.Name ?? "Unknown");
                throw new UnauthorizedAccessException("User ID cannot be determined from the token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.UserId == authenticatedUserId)
                    .ToListAsync();

                if (categories == null || categories.Count == 0)
                {
                    _logger.LogWarning("No categories found for user ID: {UserId}", authenticatedUserId);
                    return NotFound("No categories found.");
                }

                _logger.LogInformation("Successfully retrieved categories for user ID: {UserId}", authenticatedUserId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories for user ID: {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while retrieving categories.");
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == authenticatedUserId && c.CategoryId == categoryId);
                
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for user ID: {UserId}", categoryId, authenticatedUserId);
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                _logger.LogInformation("Successfully retrieved category with ID {CategoryId} for user ID: {UserId}", categoryId, authenticatedUserId);
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId} for user ID: {UserId}", categoryId, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while retrieving the category.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var newCategory = new CategoryModel
                {
                    UserId = authenticatedUserId,
                    Name = categoryDto.Name,
                    Type = categoryDto.Type
                };

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created category for user ID: {UserId}", authenticatedUserId);
                return CreatedAtAction(nameof(GetCategory), new { categoryId = newCategory.CategoryId }, newCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category for user ID: {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while creating the category.");
            }
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == authenticatedUserId && c.CategoryId == categoryId);

                if (existingCategory == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for user ID: {UserId}", categoryId, authenticatedUserId);
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                existingCategory.Name = categoryDto.Name;
                existingCategory.Type = categoryDto.Type;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated category with ID {CategoryId} for user ID: {UserId}", categoryId, authenticatedUserId);
                return Ok(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {CategoryId} for user ID: {UserId}", categoryId, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while updating the category.");
            }
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                int authenticatedUserId = GetAuthenticatedUserId();

                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == authenticatedUserId && c.CategoryId == categoryId);
                
                if (existingCategory == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for user ID: {UserId}", categoryId, authenticatedUserId);
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                _context.Categories.Remove(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted category with ID {CategoryId} for user ID: {UserId}", categoryId, authenticatedUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {CategoryId} for user ID: {UserId}", categoryId, GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while deleting the category.");
            }
        }
    }
}

using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller.Employees
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DepartmentController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(MyDataContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments.ToListAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("department/{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department by ID.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("department")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentModel department)
        {
            if (department == null)
            {
                return BadRequest("Department data is null.");
            }
            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetDepartmentById),
                    new { id = department.DepartmentId },
                    department
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("department/{id}")]
        public async Task<IActionResult> UpdateDepartment(
            int id,
            [FromBody] DepartmentModel department
        )
        {
            if (department == null || department.DepartmentId != id)
            {
                return BadRequest("Department data is null or ID mismatch.");
            }
            try
            {
                var existingDepartment = await _context.Departments.FindAsync(id);
                if (existingDepartment == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                existingDepartment.Name = department.Name;
                existingDepartment.Description = department.Description;
                existingDepartment.UpdatedAtUtc = DateTime.UtcNow;
                existingDepartment.UpdatedBy = User.Identity?.Name ?? "Unknown";
                _context.Departments.Update(existingDepartment);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("department/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

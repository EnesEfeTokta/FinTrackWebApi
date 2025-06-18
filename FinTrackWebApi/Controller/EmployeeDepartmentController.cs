using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeeDepartmentController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<EmployeeDepartmentController> _logger;

        public EmployeeDepartmentController(
            MyDataContext context,
            ILogger<EmployeeDepartmentController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("employee-departments")]
        public async Task<IActionResult> GetEmployeeDepartments()
        {
            try
            {
                var employeeDepartments = await _context
                    .EmployeeDepartments.Include(ed => ed.Employee)
                    .Include(ed => ed.Department)
                    .ToListAsync();
                return Ok(employeeDepartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee departments.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("employee-department/{id}")]
        public async Task<IActionResult> GetEmployeeDepartmentById(int id)
        {
            try
            {
                var employeeDepartment = await _context
                    .EmployeeDepartments.Include(ed => ed.Employee)
                    .Include(ed => ed.Department)
                    .FirstOrDefaultAsync(ed => ed.EmployeeDepartmentId == id);

                if (employeeDepartment == null)
                {
                    return NotFound($"Employee Department with ID {id} not found.");
                }
                return Ok(employeeDepartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee department by ID.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("employee-department")]
        public async Task<IActionResult> CreateEmployeeDepartment(
            [FromBody] EmployeeDepartmentModel employeeDepartment
        )
        {
            if (employeeDepartment == null)
            {
                return BadRequest("Employee Department data is null.");
            }
            try
            {
                _context.EmployeeDepartments.Add(employeeDepartment);

                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetEmployeeDepartmentById),
                    new { id = employeeDepartment.EmployeeDepartmentId },
                    employeeDepartment
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee department.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("employee-department/{id}")]
        public async Task<IActionResult> UpdateEmployeeDepartment(
            int id,
            [FromBody] EmployeeDepartmentModel employeeDepartment
        )
        {
            if (employeeDepartment == null || employeeDepartment.EmployeeDepartmentId != id)
            {
                return BadRequest("Employee Department data is null or ID mismatch.");
            }
            try
            {
                var existingEmployeeDepartment = await _context.EmployeeDepartments.FindAsync(id);
                if (existingEmployeeDepartment == null)
                {
                    return NotFound($"Employee Department with ID {id} not found.");
                }

                existingEmployeeDepartment.EmployeeId = employeeDepartment.EmployeeId;
                existingEmployeeDepartment.DepartmentId = employeeDepartment.DepartmentId;
                existingEmployeeDepartment.Notes = employeeDepartment.Notes;
                existingEmployeeDepartment.UpdatedAtUtc = DateTime.UtcNow;
                existingEmployeeDepartment.UpdatedBy = employeeDepartment.UpdatedBy;

                _context.EmployeeDepartments.Update(existingEmployeeDepartment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee department.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("employee-department/{id}")]
        public async Task<IActionResult> DeleteEmployeeDepartment(int id)
        {
            try
            {
                var employeeDepartment = await _context.EmployeeDepartments.FindAsync(id);
                if (employeeDepartment == null)
                {
                    return NotFound($"Employee Department with ID {id} not found.");
                }
                _context.EmployeeDepartments.Remove(employeeDepartment);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee department.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

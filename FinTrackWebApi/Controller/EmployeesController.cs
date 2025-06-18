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
    public class EmployeesController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(MyDataContext context, ILogger<EmployeesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesAsync()
        {
            try
            {
                var employees = await _context.Employees.ToListAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found.");
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by ID.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeAsync([FromBody] EmployeesModel employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee data is null.");
            }
            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetEmployeeByIdAsync),
                    new { id = employee.EmployeeId },
                    employee
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployeeAsync(
            int id,
            [FromBody] EmployeesModel employee
        )
        {
            if (employee == null || employee.EmployeeId != id)
            {
                return BadRequest("Employee data is null or ID mismatch.");
            }
            try
            {
                var existingEmployee = await _context.Employees.FindAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound($"Employee with ID {id} not found.");
                }
                existingEmployee.Name = employee.Name;
                existingEmployee.Email = employee.Email;
                existingEmployee.PhoneNumber = employee.PhoneNumber;
                existingEmployee.EmployeeStatus = employee.EmployeeStatus;
                existingEmployee.HireDate = employee.HireDate;
                existingEmployee.Salary = employee.Salary;
                existingEmployee.IsActive = employee.IsActive;
                existingEmployee.UpdatedAtUtc = DateTime.UtcNow;
                existingEmployee.UpdatedBy = employee.UpdatedBy;
                _context.Employees.Update(existingEmployee);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeAsync(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found.");
                }
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

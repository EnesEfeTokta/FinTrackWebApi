using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeManagementController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<EmployeeManagementController> _logger;

        public EmployeeManagementController(
            MyDataContext context,
            ILogger<EmployeeManagementController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        // Admin rolünde sahip kullanıcılar için çalışan ekleme ve listeleme işlemleri
        [HttpPost("register-employee")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeesModel employeeDto)
        {
            if (await _context.Employees.AnyAsync(e => e.Email == employeeDto.Email))
            {
                _logger.LogWarning(
                    "Employee registration initiation failed: Email {Email} already exists.",
                    employeeDto.Email
                );
                return BadRequest("This email address is already registered.");
            }

            try
            {
                await _context.Employees.AddAsync(employeeDto);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to initiate employee registration for {Email}.",
                    employeeDto.Email
                );
                return StatusCode(500, "An error occurred while initiating employee registration.");
            }
        }

        // Video Onay rolünde çalışanların video onaylama işlemleri
        [HttpGet("debt-all")]
        [Authorize(Roles = "VideoApproval,Admin")]
        public async Task<IActionResult> GetAllDebts()
        {
            try
            {
                var debts = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .ToListAsync();
                return Ok(debts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all debts.");
                return StatusCode(500, "An error occurred while retrieving debts.");
            }
        }

        [HttpGet("debt-type-{status}")]
        [Authorize(Roles = "VideoApproval,Admin")]
        public async Task<IActionResult> GetPendingDebts([FromQuery] string status)
        {
            DebtStatus debtStatus = (DebtStatus)Enum.Parse(typeof(DebtStatus), status, true);
            if (!Enum.IsDefined(typeof(DebtStatus), debtStatus))
            {
                _logger.LogWarning("Invalid debt status: {Status}", status);
                return BadRequest("Invalid debt status provided.");
            }

            try
            {
                var pendingDebts = await _context
                    .Debts.Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.Status == debtStatus)
                    .ToListAsync();
                return Ok(pendingDebts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve pending debts.");
                return StatusCode(500, "An error occurred while retrieving pending debts.");
            }
        }

        [HttpPost("debt-approval/{id}")]
        [Authorize(Roles = "VideoApproval,Admin")]
        public async Task<IActionResult> UpdateEmployee(
            [FromRoute] int id,
            [FromQuery] bool isApproval = false
        )
        {
            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
            {
                _logger.LogWarning("Debt with ID {Id} not found.", id);
                return NotFound("Debt not found.");
            }
            if (debt.Status != DebtStatus.PendingOperatorApproval)
            {
                _logger.LogWarning(
                    "Debt with ID {Id} is not in a valid state for approval or rejection.",
                    id
                );
                return BadRequest("Debt is not in a valid state for approval or rejection.");
            }
            try
            {
                if (isApproval)
                {
                    debt.Status = DebtStatus.Active;
                    _logger.LogInformation("Debt with ID {Id} has been approved.", id);
                }
                else
                {
                    debt.Status = DebtStatus.RejectedByOperator;
                    _logger.LogInformation("Debt with ID {Id} has been rejected.", id);
                }

                debt.UpdatedAtUtc = DateTime.UtcNow;

                _context.Debts.Update(debt);
                await _context.SaveChangesAsync();

                return Ok(debt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update employee with ID {Id}.", id);
                return StatusCode(500, "An error occurred while updating the employee.");
            }
        }
    }
}

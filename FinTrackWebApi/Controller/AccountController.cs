using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.AccountDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class AccountController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(MyDataContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var accountsFromDb = await _context
                    .Accounts.Where(a => a.UserId == userId)
                    .OrderBy(a => a.Name)
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Type,
                        a.IsActive,
                        a.CreatedAtUtc,
                        a.UpdatedAtUtc,
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var accountDtos = new List<AccountDto>();
                foreach (var acc in accountsFromDb)
                {
                    accountDtos.Add(
                        new AccountDto
                        {
                            AccountId = acc.Id,
                            Name = acc.Name,
                            Type = acc.Type,
                            IsActive = acc.IsActive,
                            Balance = await CalculateBalanceAsync(acc.Id),
                            CreatedAtUtc = acc.CreatedAtUtc,
                            UpdatedAtUtc = acc.UpdatedAtUtc,
                        }
                    );
                }

                return Ok(accountDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving accounts for user ID: {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving accounts.");
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int Id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var accountFromDb = await _context
                    .Accounts.Where(a => a.Id == Id && a.UserId == userId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Type,
                        a.IsActive,
                        a.CreatedAtUtc,
                        a.UpdatedAtUtc,
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (accountFromDb == null)
                {
                    _logger.LogWarning(
                        "Account with ID {Id} not found for user ID: {UserId}",
                        Id,
                        userId
                    );
                    return NotFound($"Account with ID {Id} not found.");
                }

                var accountDto = new AccountDto
                {
                    AccountId = accountFromDb.Id,
                    Name = accountFromDb.Name,
                    Type = accountFromDb.Type,
                    IsActive = accountFromDb.IsActive,
                    Balance = await CalculateBalanceAsync(accountFromDb.Id),
                    CreatedAtUtc = accountFromDb.CreatedAtUtc,
                    UpdatedAtUtc = accountFromDb.UpdatedAtUtc,
                };

                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving account with ID {Id} for user ID: {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while retrieving the account.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountCreateDto accountDto)
        {
            if (accountDto == null)
            {
                return BadRequest("Account data is required.");
            }

            try
            {
                int userId = GetAuthenticatedUserId();

                var account = new AccountModel
                {
                    UserId = userId,
                    Name = accountDto.Name,
                    Type = accountDto.Type,
                    Balance = accountDto.Balance,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetAccount),
                    new { Id = account.Id },
                    account
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating account for user {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while creating the account.");
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateAccount(
            int Id,
            [FromBody] AccountUpdateDto accountDto
        )
        {
            if (accountDto == null)
            {
                return BadRequest("Account data is required.");
            }
            try
            {
                int userId = GetAuthenticatedUserId();

                var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                    a.Id == Id && a.UserId == userId
                );

                if (account == null)
                {
                    return NotFound($"Account with ID {Id} not found for user {userId}.");
                }

                account.Name = accountDto.Name;
                account.Type = accountDto.Type;
                account.Balance = accountDto.Balance;
                account.UpdatedAtUtc = DateTime.UtcNow;

                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating account with ID {Id} for user {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while updating the account.");
            }
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteAccount(int Id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                    a.Id == Id && a.UserId == userId
                );

                if (account == null)
                {
                    return NotFound($"Account with ID {Id} not found for user {userId}.");
                }

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting account with ID {Id} for user {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "An error occurred while deleting the account.");
            }
        }

        private async Task<decimal> CalculateBalanceAsync(int Id)
        {
            var balance = await _context
                .Transactions.Where(t => t.Id == Id)
                .Include(t => t.Category)
                .SumAsync(t => t.Category.Type == TransactionCategoryType.Income ? t.Amount : -t.Amount);

            return balance;
        }
    }
}

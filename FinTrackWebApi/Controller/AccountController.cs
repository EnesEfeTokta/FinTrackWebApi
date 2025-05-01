using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

                var accountsFromDb = await _context.Accounts
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.Name)
                    .Select(a => new
                    {
                        a.AccountId,
                        a.Name,
                        a.Type,
                        a.IsActive,
                        a.CreatedAtUtc,
                        a.UpdatedAtUtc
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var accountDtos = new List<AccountDto>();
                foreach (var acc in accountsFromDb)
                {
                    accountDtos.Add(new AccountDto
                    {
                        AccountId = acc.AccountId,
                        Name = acc.Name,
                        Type = acc.Type,
                        IsActive = acc.IsActive,
                        Balance = await CalculateBalanceAsync(acc.AccountId),
                        CreatedAtUtc = acc.CreatedAtUtc,
                        UpdatedAtUtc = acc.UpdatedAtUtc
                    });
                }

                return Ok(accountDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for user ID: {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while retrieving accounts.");
            }
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int accountId)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var accountFromDb = await _context.Accounts
                    .Where(a => a.AccountId == accountId && a.UserId == userId)
                     .Select(a => new
                     {
                         a.AccountId,
                         a.Name,
                         a.Type,
                         a.IsActive,
                         a.CreatedAtUtc,
                         a.UpdatedAtUtc
                     })

                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (accountFromDb == null)
                {
                    _logger.LogWarning("Account with ID {AccountId} not found for user ID: {UserId}", accountId, userId);
                    return NotFound($"Account with ID {accountId} not found.");
                }

                var accountDto = new AccountDto
                {
                    AccountId = accountFromDb.AccountId,
                    Name = accountFromDb.Name,
                    Type = accountFromDb.Type,
                    IsActive = accountFromDb.IsActive,
                    Balance = await CalculateBalanceAsync(accountFromDb.AccountId),
                    CreatedAtUtc = accountFromDb.CreatedAtUtc,
                    UpdatedAtUtc = accountFromDb.UpdatedAtUtc
                };

                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account with ID {AccountId} for user ID: {UserId}", accountId, GetAuthenticatedUserId());
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
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAccount), new { accountId = account.AccountId }, account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while creating the account.");
            }
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> UpdateAccount(int accountId, [FromBody] AccountUpdateDto accountDto)
        {
            if (accountDto == null)
            {
                return BadRequest("Account data is required.");
            }
            try
            {
                int userId = GetAuthenticatedUserId();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

                if (account == null)
                {
                    return NotFound($"Account with ID {accountId} not found for user {userId}.");
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
                _logger.LogError(ex, "Error updating account with ID {AccountId} for user {UserId}", accountId, GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while updating the account.");
            }
        }

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

                if (account == null)
                {
                    return NotFound($"Account with ID {accountId} not found for user {userId}.");
                }

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account with ID {AccountId} for user {UserId}", accountId, GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while deleting the account.");
            }
        }

        private async Task<decimal> CalculateBalanceAsync(int accountId)
        {
            var balance = await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .Include(t => t.Category)
                .SumAsync(t => t.Category.Type == CategoryType.Income ? t.Amount : -t.Amount);

            return balance;
        }
    }
}

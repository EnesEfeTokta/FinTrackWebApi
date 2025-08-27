using FinTrackWebApi.Dtos.AccountDtos;
using FinTrackWebApi.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Accounts
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
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
                var accountDtos = await _accountService.GetAccountsAsync(userId);
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
                var accountDto = await _accountService.GetAccountByIdAsync(Id, userId);
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
                var createdAccount = await _accountService.CreateAccountAsync(accountDto, userId);
                return CreatedAtAction(
                    nameof(GetAccount),
                    new { Id = createdAccount.Id },
                    createdAccount
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
                bool updateStatus = await _accountService.UpdateAccountAsync(Id, accountDto, userId);
                return Ok(updateStatus);
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
                bool deleteStatus = await _accountService.DeleteAccountAsync(Id, userId);
                return Ok(deleteStatus);
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
    }
}

using FinTrackWebApi.Dtos.AccountDtos;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Account;
using FinTrackWebApi.Repositories.Account;

namespace FinTrackWebApi.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<AccountDto> CreateAccountAsync(AccountCreateDto accountDto, int userId)
        {
            var account = new AccountModel
            {
                UserId = userId,
                Name = accountDto.Name,
                Type = accountDto.Type,
                IsActive = accountDto.IsActive,
                Currency = accountDto.Currency,
                CreatedAtUtc = DateTime.UtcNow,
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                Name = account.Name,
                Type = account.Type ?? AccountType.Error,
                IsActive = account.IsActive,
                Balance = account.Balance,
                Currency = account.Currency ?? BaseCurrencyType.Error,
                CreatedAtUtc = account.CreatedAtUtc
            };
        }

        public async Task<bool> DeleteAccountAsync(int id, int userId)
        {
            var account = await _accountRepository.GetByIdAndUserIdAsync(id, userId);

            if (account == null)
            {
                return false;
            }

            _accountRepository.Delete(account);
            return await _accountRepository.SaveChangesAsync();
        }

        public async Task<AccountDto> GetAccountByIdAsync(int id, int userId)
        {
            var accountFromDb = await _accountRepository.GetByIdAndUserIdAsync(id, userId);

            if (accountFromDb == null)
            {
                return null;
            }

            return new AccountDto
            {
                Id = accountFromDb.Id,
                Name = accountFromDb.Name,
                Type = accountFromDb.Type ?? AccountType.Error,
                IsActive = accountFromDb.IsActive,
                Balance = accountFromDb.Balance,
                Currency = accountFromDb.Currency ?? BaseCurrencyType.Error,
                CreatedAtUtc = accountFromDb.CreatedAtUtc,
                UpdatedAtUtc = accountFromDb.UpdatedAtUtc,
            };
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsAsync(int userId)
        {
            var accountsFromDb = await _accountRepository.GetByUserIdAsync(userId);

            if (accountsFromDb == null)
            {
                return Enumerable.Empty<AccountDto>();
            }

            return accountsFromDb.Select(acc => new AccountDto
            {
                Id = acc.Id,
                Name = acc.Name,
                Type = acc.Type ?? AccountType.Error,
                IsActive = acc.IsActive,
                Balance = acc.Balance,
                Currency = acc.Currency ?? BaseCurrencyType.Error,
                CreatedAtUtc = acc.CreatedAtUtc,
                UpdatedAtUtc = acc.UpdatedAtUtc,
            });
        }

        public async Task<bool> UpdateAccountAsync(int id, AccountUpdateDto accountDto, int userId)
        {
            var account = await _accountRepository.GetByIdAndUserIdAsync(id, userId);

            if (account == null)
            {
                return false;
            }

            account.Name = accountDto.Name;
            account.Type = accountDto.Type;
            account.Currency = accountDto.Currency;
            account.UpdatedAtUtc = DateTime.UtcNow;

            _accountRepository.Update(account);
            return await _accountRepository.SaveChangesAsync();
        }
    }
}

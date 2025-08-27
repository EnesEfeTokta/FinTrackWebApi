using FinTrackWebApi.Dtos.AccountDtos;

namespace FinTrackWebApi.Services.Account
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAccountsAsync(int userId);
        Task<AccountDto> GetAccountByIdAsync(int id, int userId);
        Task<AccountDto> CreateAccountAsync(AccountCreateDto accountDto, int userId);
        Task<bool> UpdateAccountAsync(int id, AccountUpdateDto accountDto, int userId);
        Task<bool> DeleteAccountAsync(int id, int userId);
    }
}

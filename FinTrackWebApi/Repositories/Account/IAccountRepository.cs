using FinTrackWebApi.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackWebApi.Repositories.Account
{
    public interface IAccountRepository
    {
        Task<IEnumerable<AccountModel>> GetByUserIdAsync(int userId);
        Task<AccountModel?> GetByIdAndUserIdAsync(int id, int userId);
        Task AddAsync(AccountModel account);
        void Update(AccountModel account);
        void Delete(AccountModel account);
        Task<bool> SaveChangesAsync();
    }
}

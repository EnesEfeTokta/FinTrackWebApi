using FinTrackWebApi.Data;
using FinTrackWebApi.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace FinTrackWebApi.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly MyDataContext _context;

        public AccountRepository(MyDataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccountModel account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public void Delete(AccountModel account)
        {
            _context.Accounts.Remove(account);
        }

        public async Task<AccountModel?> GetByIdAndUserIdAsync(int id, int userId)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<IEnumerable<AccountModel>> GetByUserIdAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AccountModel account)
        {
            _context.Accounts.Update(account);
        }
    }
}

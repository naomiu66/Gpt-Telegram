using Gpt_Telegram.Data.Models.Telegram;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gpt_Telegram.Data.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationContext _context;

        public UsersRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<User> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var user = await _context.Users
                .Include(u => u.ActiveSession)
                .FirstOrDefaultAsync(u => u.Id == id, ct);
            return user;
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync(ct) > 0;
        }
    }
}

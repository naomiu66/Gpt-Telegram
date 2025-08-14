using Gpt_Telegram.Data.Models.Telegram;

namespace Gpt_Telegram.Data.Repositories.Abstractions
{
    public interface IUsersRepository
    {
        public Task<User> GetByIdAsync(long id, CancellationToken ct = default);
        public Task<bool> CreateAsync(User user, CancellationToken ct = default);
        public Task<bool> UpdateAsync(User user, CancellationToken ct = default);
        public Task<bool> DeleteAsync(long id, CancellationToken ct = default);
    }
}

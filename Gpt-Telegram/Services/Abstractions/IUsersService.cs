using Gpt_Telegram.Data.Models.Telegram;

namespace Gpt_Telegram.Services.Abstractions
{
    public interface IUsersService
    {
        public Task<User> GetByIdAsync(long id, CancellationToken ct = default);
        public Task<bool> CreateAsync(User user, CancellationToken ct = default);
        public Task<bool> UpdateAsync(User user, CancellationToken ct = default);
        public Task<bool> DeleteAsync(long id, CancellationToken ct = default);
    }
}

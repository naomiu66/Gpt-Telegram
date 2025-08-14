using Gpt_Telegram.Data.Models.Telegram;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Gpt_Telegram.Services.Abstractions;

namespace Gpt_Telegram.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;

        public UsersService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> CreateAsync(User user, CancellationToken ct = default)
        {
            return await _usersRepository.CreateAsync(user, ct);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
        {
            return await _usersRepository.DeleteAsync(id, ct);
        }

        public async Task<User> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await _usersRepository.GetByIdAsync(id, ct);
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
        {
            return await _usersRepository.UpdateAsync(user, ct);
        }
    }
}

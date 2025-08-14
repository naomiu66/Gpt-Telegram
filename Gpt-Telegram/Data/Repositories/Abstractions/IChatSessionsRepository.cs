using Gpt_Telegram.Data.Models.OpenAI;

namespace Gpt_Telegram.Data.Repositories.Abstractions
{
    public interface IChatSessionsRepository
    {
        public Task<List<ChatSession>> GetUserSessionsAsync(long userId, CancellationToken ct = default);
        public Task<ChatSession> GetByIdAsync(Guid id, CancellationToken ct = default);
        public Task<bool> CreateAsync(ChatSession session, CancellationToken ct = default);
        public Task<bool> UpdateAsync(ChatSession session, CancellationToken ct = default);
        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

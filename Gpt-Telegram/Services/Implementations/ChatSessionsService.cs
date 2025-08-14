using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Gpt_Telegram.Services.Abstractions;

namespace Gpt_Telegram.Services.Implementations
{
    public class ChatSessionsService : IChatSessionsService
    {
        private readonly IChatSessionsRepository _chatSessionsRepository;

        public ChatSessionsService(IChatSessionsRepository chatSessionsRepository)
        {
            _chatSessionsRepository = chatSessionsRepository;
        }

        public async Task<bool> CreateAsync(ChatSession session, CancellationToken ct = default)
        {
            return await _chatSessionsRepository.CreateAsync(session, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _chatSessionsRepository.DeleteAsync(id, ct);
        }

        public async Task<ChatSession> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _chatSessionsRepository.GetByIdAsync(id, ct);
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(long userId, CancellationToken ct = default)
        {
            return await _chatSessionsRepository.GetUserSessionsAsync(userId, ct);
        }

        public async Task<bool> UpdateAsync(ChatSession session, CancellationToken ct = default)
        {
            return await _chatSessionsRepository.UpdateAsync(session, ct);
        }
    }
}

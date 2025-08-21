using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Gpt_Telegram.Services.Abstractions;

namespace Gpt_Telegram.Services.Implementations
{
    public class ChatMessagesService : IChatMessagesService
    {
        private readonly IChatMessagesRepository _chatMessagesRepository;

        public ChatMessagesService(IChatMessagesRepository chatMessagesRepository)
        {
            _chatMessagesRepository = chatMessagesRepository;
        }

        public async Task<bool> CreateAsync(ChatMessage message, CancellationToken ct = default)
        {
            return await _chatMessagesRepository.CreateAsync(message, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _chatMessagesRepository.DeleteAsync(id, ct);
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _chatMessagesRepository.GetChatMessagesAsync(sessionId, ct);
        }

        public async Task<List<ChatMessage>> GetLastMessagesAsync(Guid sessionId, CancellationToken ct = default, int limit = 10)
        {
            return await _chatMessagesRepository.GetLastMessagesAsync(sessionId, ct, limit);
        }
    }
}

using Gpt_Telegram.Data.Models.OpenAI;

namespace Gpt_Telegram.Services.Abstractions
{
    public interface IChatMessagesService
    {
        public Task<bool> CreateAsync(ChatMessage message, CancellationToken ct = default);
        public Task<List<ChatMessage>> GetChatMessagesAsync(Guid sessionId, CancellationToken ct = default);
        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

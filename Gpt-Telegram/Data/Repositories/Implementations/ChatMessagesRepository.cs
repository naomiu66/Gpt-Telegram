using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gpt_Telegram.Data.Repositories.Implementations
{
    public class ChatMessagesRepository : IChatMessagesRepository
    {
        private readonly ApplicationContext _context;

        public ChatMessagesRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(ChatMessage message, CancellationToken ct = default)
        {
            await _context.ChatMessages.AddAsync(message);
            return await _context.SaveChangesAsync(ct) > 0; 
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message == null) return false;
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _context.ChatMessages
                .Where(cm => cm.SessionId == sessionId)
                .ToListAsync(ct);
        }
    }
}

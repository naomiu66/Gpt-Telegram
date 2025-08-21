using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gpt_Telegram.Data.Repositories.Implementations
{
    public class ChatSessionsRepository : IChatSessionsRepository
    {
        private readonly ApplicationContext _context;

        public ChatSessionsRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(ChatSession session, CancellationToken ct = default)
        {
            await _context.ChatSessions.AddAsync(session);
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var session = await _context.ChatSessions.FirstOrDefaultAsync(cs => cs.Id == id, ct);
            if (session == null) return false;

            _context.ChatSessions.Remove(session);
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<ChatSession> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == id, ct);
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(long userId, CancellationToken ct = default)
        {
            return await _context.ChatSessions
                .Where(cs => cs.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(ChatSession session, CancellationToken ct = default)
        {
            _context.ChatSessions.Update(session);
            return await _context.SaveChangesAsync(ct) > 0;
        }
    }
}

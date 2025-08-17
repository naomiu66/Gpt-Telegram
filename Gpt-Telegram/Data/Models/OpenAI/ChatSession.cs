using Gpt_Telegram.Data.Models.Telegram;

namespace Gpt_Telegram.Data.Models.OpenAI
{
    public class ChatSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Title { get; set; }
        public long UserId { get; set; }
        public string? SystemPrompt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage>? ChatMessages { get; set; } = new List<ChatMessage>();
        public User? User { get; set; }
    }
}

using OpenAI.Chat;

namespace Gpt_Telegram.Data.Models.OpenAI
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public ChatMessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ChatSession? Session { get; set; }
    }
}

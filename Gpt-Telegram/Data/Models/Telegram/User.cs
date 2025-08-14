using Gpt_Telegram.Data.Models.OpenAI;

namespace Gpt_Telegram.Data.Models.Telegram
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid ActiveSessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ChatSession ActiveSession { get; set; }
        public ICollection<ChatSession>? Sessions { get; set; } = new List<ChatSession>();
    }
}

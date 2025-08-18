using OpenAI.Chat;

namespace Gpt_Telegram.Utilities.OpenAI
{
    public interface ITokenOptimizer
    {
        public Task<List<ChatMessage>> OptimizeMessageTokens(List<ChatMessage> messages);
    }
}

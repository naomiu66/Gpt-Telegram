using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Abstractions
{
    public interface ICallbackHandler
    {
        public Task HandleAsync(CallbackQuery callback, CancellationToken ct);
    }
}

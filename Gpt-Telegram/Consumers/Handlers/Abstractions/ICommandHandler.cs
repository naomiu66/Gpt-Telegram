using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Abstractions
{
    public interface ICommandHandler
    {
        public Task HandleAsync(Update update, CancellationToken cancellationToken);
    }
}

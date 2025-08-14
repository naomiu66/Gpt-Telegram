using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class CommandHandler : ICommandHandler
    {
        public Task HandleAsync(Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

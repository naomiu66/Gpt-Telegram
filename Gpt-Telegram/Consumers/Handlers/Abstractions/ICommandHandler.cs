using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Abstractions
{
    public interface ICommandHandler
    {
        public Task HandleAsync(Update update, CancellationToken cancellationToken);

        public Task ListCommand(long chatId, CancellationToken cancellationToken, int page = 0);
    }
}

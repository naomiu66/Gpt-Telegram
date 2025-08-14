using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Services.Abstractions;
using Gpt_Telegram.Services.Implementations;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class PromptHandler : IPromptHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUsersService _usersService;
        private readonly IChatSessionsService _chatSessionsService;
        private readonly IChatMessagesService _chatMessagesService;

        public PromptHandler(ITelegramBotClient botClient,
            IUsersService usersService,
            IChatSessionsService chatSessionsService,
            IChatMessagesService chatMessagesService)
        {
            _botClient = botClient;
            _usersService = usersService;
            _chatSessionsService = chatSessionsService;
            _chatMessagesService = chatMessagesService;
        }

        public Task HandleAsync(Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

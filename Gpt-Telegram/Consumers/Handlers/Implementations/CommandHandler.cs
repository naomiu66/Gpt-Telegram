using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Data.Redis.Repositories;
using Gpt_Telegram.Pipelines;
using Gpt_Telegram.Services.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class CommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUsersService _usersService;
        private readonly IChatSessionsService _sessionsService;
        private readonly PipelineRouter _pipelineRouter;
        private readonly IUserStateRepository _userStateRepository;

        public CommandHandler(ITelegramBotClient botClient,
            IUsersService usersService,
            IChatSessionsService sessionsService,
            PipelineRouter pipelineRouter,
            IUserStateRepository userStateRepository)
        {
            _botClient = botClient;
            _usersService = usersService;
            _sessionsService = sessionsService;
            _pipelineRouter = pipelineRouter;
            _userStateRepository = userStateRepository;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken)
        {
            var message = update.Message.Text;
            var chatId = update.Message.From.Id;
            var username = update.Message.From.Username;
            var firstname = update.Message.From.FirstName;
            var lastname = update.Message.From.LastName;
            switch (message)
            {
                case "/start":
                    await StartCommand(chatId, username, firstname, lastname, cancellationToken);
                    break;
                case "/help":
                    await HelpCommand(chatId, cancellationToken);
                    break;
                case "/cancel":
                    await CancelCommand();
                    break;
                case "/list":
                    await ListCommand();
                    break;
                case "/new":
                    await NewSessionCommand(chatId, cancellationToken);
                    break;

                default:
                    break;
            }
        }

        private async Task ListCommand()
        {
            throw new NotImplementedException();
        }

        private async Task NewSessionCommand(long chatId, CancellationToken cancellationToken)
        {
            var state = await _userStateRepository.GetStateAsync(chatId);

            if(state == null)
            {
                await _userStateRepository.UpdateStepAsync(chatId, "SessionCreation", "SetTitle", new Dictionary<string, object>());
                await _botClient.SendMessage(chatId, "Введите название для новой сессии.", cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Вы уже создаете новую сессию", cancellationToken: cancellationToken);
            }
        }

        private async Task CancelCommand()
        {
            throw new NotImplementedException();
        }

        private async Task HelpCommand(long chatId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task StartCommand(long chatId, string? username, string firstname, string? lastname, CancellationToken cancellationToken)
        {
            var user = await _usersService.GetByIdAsync(chatId, cancellationToken);

            if (user != null)
            {
                await _botClient.SendMessage(chatId,
                    "Снова привет)",
                    cancellationToken: cancellationToken);
            }

            user = new Data.Models.Telegram.User
            {
                Id = chatId,
                Username = username,
                FirstName = firstname ?? string.Empty,
                LastName = lastname ?? string.Empty,
                ActiveSessionId = null
            };

            var response = await _usersService.CreateAsync(user, cancellationToken);

            if (response) 
            {
                await _botClient.SendMessage(chatId,
                    "Привет! Я бот, который поможет тебе в работе с OpenAI.",
                    cancellationToken: cancellationToken);
                await NewSessionCommand(chatId, cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(chatId,
                    "Произошла ошибка при создании пользователя. Пожалуйста, попробуйте еще раз позже.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}

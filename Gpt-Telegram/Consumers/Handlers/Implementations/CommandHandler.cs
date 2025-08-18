using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Data.Redis.Repositories;
using Gpt_Telegram.Pipelines;
using Gpt_Telegram.Services.Abstractions;
using Gpt_Telegram.Utilities.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class CommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUsersService _usersService;
        private readonly IChatSessionsService _sessionsService;
        private readonly IUserStateRepository _userStateRepository;
        private readonly KeyboardMarkupBuilder _keyboardMarkupBuilder;

        public CommandHandler(ITelegramBotClient botClient,
            IUsersService usersService,
            IChatSessionsService sessionsService,
            IUserStateRepository userStateRepository,
            KeyboardMarkupBuilder keyboardMarkupBuilder)
        {
            _botClient = botClient;
            _usersService = usersService;
            _sessionsService = sessionsService;
            _userStateRepository = userStateRepository;
            _keyboardMarkupBuilder = keyboardMarkupBuilder;
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
                    await ListCommand(chatId, cancellationToken);
                    break;
                case "/new":
                    await NewSessionCommand(chatId, cancellationToken);
                    break;

                default:
                    break;
            }
        }

        public async Task ListCommand(long chatId, CancellationToken cancellationToken, int page = 0)
        {
            var sessions = await _sessionsService.GetUserSessionsAsync(chatId);
            var user = await _usersService.GetByIdAsync(chatId);

            if(sessions == null)
            {
                await _botClient.SendMessage(chatId, "У вас нет активных сессий. Пожалуйста, создайте новую сессию с помощью команды /new.", cancellationToken: cancellationToken);
                return;
            }

            sessions = sessions
                .OrderByDescending(s => s.Id == user.ActiveSessionId)
                .ThenByDescending(s => s.CreatedAt)
                .ToList();
            List<InlineKeyboardButton> buttons = new();

            var pages = ChunkBy(sessions, 5).ToList();
            if (page >= pages.Count) page = pages.Count() - 1;

            foreach (var session in pages[page])
            {
                var text = session.Id == user.ActiveSessionId
                    ? $"{session.Title} (Активная)"
                    : session.Title;

                buttons.Add(_keyboardMarkupBuilder.InitializeKeyboardButton(text, $"selectSession:{session.Id}"));
            }

            List<InlineKeyboardButton> paginationButtons = new();
            if (page > 0)
                paginationButtons.Add(_keyboardMarkupBuilder.InitializeKeyboardButton("⬅️ Назад", $"sessions_page:{page - 1}"));

            if (page < pages.Count - 1)
                paginationButtons.Add(_keyboardMarkupBuilder.InitializeKeyboardButton("Вперед ➡️", $"sessions_page:{page + 1}"));
            paginationButtons.Add(_keyboardMarkupBuilder.InitializeKeyboardButton("Выход", "cancel"));

            var markup = _keyboardMarkupBuilder.InitializeKeyboardMarkup(
                buttons.Concat(paginationButtons).ToList(),
                buttonsPerRow: 1
            );

            await _botClient.SendMessage(
                chatId,
                $"Ваши сессии (страница {page + 1}/{pages.Count}):",
                replyMarkup: markup,
                cancellationToken: cancellationToken
            );
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
                return;
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
                    "Привет! Я бот, который поможет тебе в работе с OpenAI. Для начала необходимо создать чат сессию.",
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

        private IEnumerable<List<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(g => g.Select(v => v.Value).ToList());
        }
    }
}

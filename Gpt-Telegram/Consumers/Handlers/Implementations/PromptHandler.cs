using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Data.Redis.Repositories;
using Gpt_Telegram.Pipelines;
using Gpt_Telegram.Services.Abstractions;
using Gpt_Telegram.Utilities.Telegram;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class PromptHandler : IPromptHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ChatClient _chatClient;
        private readonly IUsersService _usersService;
        private readonly IChatSessionsService _chatSessionsService;
        private readonly IChatMessagesService _chatMessagesService;
        private readonly IUserStateRepository _userStateRepository;
        private readonly PipelineRouter _pipelineRouter;
        private readonly ITelegramFormatter _telegramFormatter;

        public PromptHandler(ITelegramBotClient botClient,
            ChatClient chatClient,
            IUsersService usersService,
            IChatSessionsService chatSessionsService,
            IChatMessagesService chatMessagesService,
            IUserStateRepository userStateRepository,
            PipelineRouter pipelineRouter,
            ITelegramFormatter telegramFormatter)
        {
            _botClient = botClient;
            _chatClient = chatClient;
            _usersService = usersService;
            _chatSessionsService = chatSessionsService;
            _chatMessagesService = chatMessagesService;
            _userStateRepository = userStateRepository;
            _pipelineRouter = pipelineRouter;
            _telegramFormatter = telegramFormatter;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken)
        {
            var userId = update.Message.From.Id;
            var message = update.Message.Text;

            var state = await _userStateRepository.GetStateAsync(userId);

            if (state != null)
            {
                var context = new PipelineContext
                {
                    UserId = userId,
                    MessageText = message,
                    StepName = state.StepName,
                    PipelineName = state.PipelineName,
                    Data = state.Data
                };

                await _pipelineRouter.RouteAsync(context, cancellationToken);
            }
            else
            {
                await PromptProcessing(userId, message, cancellationToken);
            }
        }

        private async Task PromptProcessing(long userId, string message, CancellationToken cancellationToken)
        {
            var user = await _usersService.GetByIdAsync(userId, cancellationToken);
            var session = user.ActiveSession;

            if (session == null)
            {
                await _botClient.SendMessage(userId, "У вас нет активной сессии. Пожалуйста, создайте новую сессию, с помощью команды /new, или выберите активную сессию из списка с помощью команды /list.", cancellationToken: cancellationToken);
                return;
            }

            List<Data.Models.OpenAI.ChatMessage> history = await _chatMessagesService.GetLastMessagesAsync(session.Id, cancellationToken);

            List<OpenAI.Chat.ChatMessage> messages = history
                .Select(m => m.Role switch
                {
                    ChatMessageRole.User => new UserChatMessage(m.Content) as OpenAI.Chat.ChatMessage,
                    ChatMessageRole.Assistant => new AssistantChatMessage(m.Content),
                    ChatMessageRole.System => new SystemChatMessage(m.Content),
                    _ => throw new ArgumentOutOfRangeException(nameof(m.Role), m.Role, null)
                })
                .ToList();

            messages.Add(new UserChatMessage(message));

            using var typingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var typingTask = StartTypingTask(userId, typingCts.Token);

            ChatCompletion completion;

            try
            {
                completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                completion = null;
            }
            finally
            {
                typingCts.Cancel();
            }

            if (completion != null)
            {
                await _chatMessagesService.CreateAsync(new Data.Models.OpenAI.ChatMessage
                {
                    Role = ChatMessageRole.User,
                    Content = message,
                    SessionId = session.Id,
                }, cancellationToken);
                await _chatMessagesService.CreateAsync(new Data.Models.OpenAI.ChatMessage
                {
                    Role = ChatMessageRole.Assistant,
                    Content = completion.Content[0].Text,
                    SessionId = session.Id,
                }, cancellationToken);

                var formattedMessage = _telegramFormatter.FormatMessage(completion.Content[0].Text);

                foreach (var chunk in formattedMessage)
                {
                    await _botClient.SendMessage(
                        userId,
                        chunk,
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                }
                Console.WriteLine($"[PromptHandler] gpt-4o : {completion.Content[0].Text}");
            }
            else
            {
                await _botClient.SendMessage(userId,
                    "Произошла ошибка при обработке вашего запроса. Пожалуйста, попробуйте еще раз позже.",
                    cancellationToken: cancellationToken);
            }
        }

        private Task StartTypingTask(long chatId, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await _botClient.SendChatAction(chatId, ChatAction.Typing, cancellationToken: cancellationToken);
                        await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }, cancellationToken);
        }
    }
}

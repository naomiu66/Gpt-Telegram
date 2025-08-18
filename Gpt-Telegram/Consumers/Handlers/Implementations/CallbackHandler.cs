using DotNetEnv;
using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Services.Abstractions;
using Gpt_Telegram.Utilities.Telegram;
using System.Runtime.CompilerServices;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers.Handlers.Implementations
{
    public class CallbackHandler : ICallbackHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUsersService _usersService;
        private readonly IChatSessionsService _chatSessionsService;
        private readonly KeyboardMarkupBuilder _keyboardMarkupBuilder;
        private readonly ICommandHandler _commandHandler;

        public CallbackHandler(
            ITelegramBotClient botClient,
            IUsersService usersService,
            IChatSessionsService chatSessionsService,
            KeyboardMarkupBuilder keyboardMarkupBuilder,
            ICommandHandler commandHandler)
        {
            _botClient = botClient;
            _usersService = usersService;
            _chatSessionsService = chatSessionsService;
            _keyboardMarkupBuilder = keyboardMarkupBuilder;
            _commandHandler = commandHandler;
        }

        public async Task HandleAsync(CallbackQuery callback, CancellationToken ct)
        {

            var parts = callback.Data.Split(':');

            var type = parts[0];
            var value = parts[1];
            var chatId = callback.From.Id;
            var messageId = callback.Message?.MessageId;

            switch (type)
            {
                case "select_session":
                    await SelectSessionAsync(value, chatId, messageId, ct);
                    break;
                case "sessions_page":
                    await ChangePageAsync(value, chatId, messageId, ct);
                    break;
                case "cancel":
                    await CancelAsync(chatId, messageId);
                    break;
                default:
                    break;
            }
        }

        private async Task CancelAsync(long chatId, int? messageId) 
        {
            Console.WriteLine($"Cancel callback received for chat {chatId}");
            await _botClient.DeleteMessage(
                chatId: chatId,
                messageId: messageId.Value
            );
        }

        private async Task ChangePageAsync(string value, long chatId, int? messageId, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Change page callback received for chat {chatId} with value {value}");
            if (int.TryParse(value, out int page))
            {
                if (messageId.HasValue)
                {
                    await _keyboardMarkupBuilder.RemoveKeybordMarkup(
                        _botClient,
                        chatId,
                        messageId.Value
                    );
                }

                await _commandHandler.ListCommand(chatId, cancellationToken, page);
            }
        }

        private async Task SelectSessionAsync(string value, long chatId, int? messageId, CancellationToken cancellationToken) 
        {
            Console.WriteLine($"Select session callback received for chat {chatId} with value {value}");
            if (Guid.TryParse(value, out Guid sessionId))
            {
                var user = await _usersService.GetByIdAsync(chatId);

                if(sessionId == user.ActiveSessionId) 
                {
                    await _botClient.SendMessage(chatId,
                        "Данная сессия уже активная.",
                        cancellationToken: cancellationToken);
                }

                user.ActiveSessionId = sessionId;

                var response = await _usersService.UpdateAsync(user, cancellationToken);

                if(!response) 
                {
                    await _botClient.SendMessage(
                        chatId,
                        "Произошла ошибка при обновлении активной сессии. Пожалуйста, попробуйте еще раз позже.",
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                var session = await _chatSessionsService.GetByIdAsync(sessionId, cancellationToken);

                if (messageId != null)
                {
                    await _botClient.DeleteMessage(
                        chatId: chatId,
                        messageId: messageId.Value
                    );
                }

                await _botClient.SendMessage(
                    chatId,
                    $"✅ Активная сессия изменена на {session.Title}",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}

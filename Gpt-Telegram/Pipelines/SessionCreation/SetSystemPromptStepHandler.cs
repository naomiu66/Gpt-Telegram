
using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Services.Abstractions;
using OpenAI.Chat;
using Telegram.Bot;

namespace Gpt_Telegram.Pipelines.SessionCreation
{
    public class SetSystemPromptStepHandler : StepHandler
    {
        private readonly IChatSessionsService _chatSessionsService;
        private readonly IUsersService _userService;
        private readonly IChatMessagesService _chatMessagesService;
        public SetSystemPromptStepHandler(ITelegramBotClient botClient,
            IChatSessionsService chatSessionsService,
            IUsersService usersService,
            IChatMessagesService chatMessagesService)
            : base("SessionCreation", "SetSystemPrompt", botClient)
        {
            _chatSessionsService = chatSessionsService;
            _userService = usersService;
            _chatMessagesService = chatMessagesService;
        }

        public override async Task HandleAsync(PipelineContext context, CancellationToken cancellationToken)
        {
            if (context.MessageText != null)
            {
                context.Data["SystemPrompt"] = context.MessageText;
                NextStepName = null;

                var session = new ChatSession
                {
                    Title = context.Data.ContainsKey("Title") ? context.Data["Title"].ToString() : "Без названия",
                    SystemPrompt = context.Data["SystemPrompt"].ToString(),
                    UserId = context.UserId,
                };

                var response = await _chatSessionsService.CreateAsync(session, cancellationToken);

                if (response)
                {
                    var user = await _userService.GetByIdAsync(context.UserId, cancellationToken);
                    if (user != null)
                    {
                        user.ActiveSessionId = session.Id;
                        await _userService.UpdateAsync(user, cancellationToken);
                    }
                    await _chatMessagesService.CreateAsync(new Data.Models.OpenAI.ChatMessage
                    {
                        Role = ChatMessageRole.System,
                        Content = session.SystemPrompt,
                        SessionId = session.Id
                    }, cancellationToken);

                    await _botClient.SendMessage(context.UserId, "Регистрация сессии завершена!", cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(context.UserId, "Не удалось создать сессию. Пожалуйста, попробуйте еще раз.", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await _botClient.SendMessage(context.UserId, "Что-то пошло не так. Пожалуйста, напишите системный промпт для новой сессии", cancellationToken: cancellationToken);
            }
        }
    }
}

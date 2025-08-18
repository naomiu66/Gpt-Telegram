
using Gpt_Telegram.Services.Abstractions;
using Telegram.Bot;

namespace Gpt_Telegram.Pipelines.SessionCreation
{
    public class SetTitleStepHandler : StepHandler
    {
        private readonly IChatSessionsService _chatSessionsService;

        public SetTitleStepHandler(ITelegramBotClient botClient,
            IChatSessionsService chatSessionsService)
            : base("SessionCreation", "SetTitle", botClient) 
        {
            _chatSessionsService = chatSessionsService;
        }
     

        public override async Task HandleAsync(PipelineContext context, CancellationToken cancellationToken)
        {
            if(context.MessageText != null)
            {
                var sessions = await _chatSessionsService.GetUserSessionsAsync(context.UserId, cancellationToken);

                if (sessions != null)
                {
                    foreach (var session in sessions)
                    {
                        if (session.Title == context.MessageText)
                        {
                            await _botClient.SendMessage(context.UserId, "Сессия с таким названием уже существует. Пожалуйста, выберите другое.", cancellationToken: cancellationToken);
                            return;
                        }
                    }
                }

                context.Data["Title"] = context.MessageText;
                NextStepName = "SetSystemPrompt";

                await _botClient.SendMessage(context.UserId, "Напишите системный промпт для новой сессии", cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(context.UserId, "Что-то пошло не так. Пожалуйста напишите название сессии", cancellationToken: cancellationToken);
            }
        }
    }
}

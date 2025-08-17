
using Telegram.Bot;

namespace Gpt_Telegram.Pipelines.SessionCreation
{
    public class SetTitleStepHandler : StepHandler
    {
        public SetTitleStepHandler(ITelegramBotClient botClient) : base("SessionCreation", "SetTitle", botClient) { }
     

        public override async Task HandleAsync(PipelineContext context, CancellationToken cancellationToken)
        {
            if(context.MessageText != null)
            {
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

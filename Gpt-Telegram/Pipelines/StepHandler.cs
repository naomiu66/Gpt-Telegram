using Telegram.Bot;

namespace Gpt_Telegram.Pipelines
{
    public abstract class StepHandler
    {
        public string PipelineName { get; }
        public string StepName { get; }
        public string NextStepName { get; protected set; }
        protected readonly ITelegramBotClient _botClient;

        protected StepHandler(string pipelineName, string stepName, ITelegramBotClient botClient)
        {
            PipelineName = pipelineName;
            StepName = stepName;
            _botClient = botClient;
        }

        public abstract Task HandleAsync(PipelineContext context, CancellationToken cancellationToken);
    }
}

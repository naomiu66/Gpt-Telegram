using Gpt_Telegram.Data.Redis.Repositories;

namespace Gpt_Telegram.Pipelines
{
    public class PipelineRouter
    {
        private readonly IEnumerable<StepHandler> _steps;
        private readonly IUserStateRepository _userStateRepository;

        public PipelineRouter(IEnumerable<StepHandler> steps, IUserStateRepository userStateRepository)
        {
            _steps = steps;
            _userStateRepository = userStateRepository;
        }

        public async Task RouteAsync(PipelineContext context, CancellationToken ct)
        {
            var handler = _steps.FirstOrDefault(s =>
                s.PipelineName == context.PipelineName &&
                s.StepName == context.StepName);

            if (handler == null)
                throw new InvalidOperationException($"No handler found for pipeline {context.PipelineName} and step {context.StepName}");

            await handler.HandleAsync(context, ct);

            if (handler.NextStepName != null)
            {
                await _userStateRepository.UpdateStepAsync(
                    context.UserId,
                    handler.PipelineName,
                    handler.NextStepName,
                    context.Data
                );
            }
            else
            {
                await _userStateRepository.ClearStateAsync(context.UserId);
            }
        }
    }
}

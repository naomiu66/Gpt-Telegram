using Gpt_Telegram.Services.Abstractions;
using Telegram.Bot;

namespace Gpt_Telegram.Consumers
{
    public class CommandConsumer : BackgroundService
    {

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

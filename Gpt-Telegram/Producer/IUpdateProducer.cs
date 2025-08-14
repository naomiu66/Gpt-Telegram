using Telegram.Bot.Types;

namespace Gpt_Telegram.Producer
{
    public interface IUpdateProducer
    {
        public Task Publish(Update update, string queueName);
    }
}

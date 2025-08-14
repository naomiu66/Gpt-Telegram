using RabbitMQ.Client;

namespace Gpt_Telegram.Data.Connections
{
    public interface IRabbitMqConnection
    {
        public Task<IConnection> GetConnectionAsync();
    }
}

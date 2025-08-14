using RabbitMQ.Client;

namespace Gpt_Telegram.Data.Connections
{
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly Task<IConnection> _connectionTask;
        private readonly IConfiguration _configuration;

        public RabbitMqConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionTask = InitializeConnectionAsync();
        }

        private async Task<IConnection> InitializeConnectionAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RABBITMQ:HOST"],
                Port = int.Parse(_configuration["RABBITMQ:PORT"]),
                UserName = _configuration["RABBITMQ:USERNAME"],
                Password = _configuration["RABBITMQ:PASSWORD"]
            };

            return await factory.CreateConnectionAsync();
        }

        public Task<IConnection> GetConnectionAsync() => _connectionTask;

        public void Dispose()
        {
            if (_connectionTask.IsCompletedSuccessfully)
            {
                var connection = _connectionTask.Result;
                if (connection != null && connection.IsOpen)
                {
                    connection.Dispose();
                }
            }
        }
    }
}


using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Data.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Consumers
{
    public class CallbackConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        protected readonly IRabbitMqConnection _connection;

        public CallbackConsumer(IServiceScopeFactory scopeFactory, IRabbitMqConnection connection)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var c = await _connection.GetConnectionAsync();
                Console.WriteLine("[RabbitMq] Connected succesfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to connect RabbitMq : {ex.Message}");
            }

            var connection = await _connection.GetConnectionAsync();
            var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(queue: "callbacks",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine("[CommandConsumer] received message");

                using var scope = _scopeFactory.CreateScope();
                var callbackHandler = scope.ServiceProvider.GetRequiredService<ICallbackHandler>();

                try
                {
                    var update = JsonSerializer.Deserialize<Update>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    using var cts = new CancellationTokenSource();
                    try
                    {
                        await callbackHandler.HandleAsync(update.CallbackQuery, cts.Token);
                    }
                    finally
                    {
                        cts.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to process message : {ex.Message}");
                }
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queue: "callbacks",
                autoAck: false,
                consumer: consumer);
        }
    }
}

using Gpt_Telegram.Data.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gpt_Telegram.Producer
{
    public class UpdateProducer : IUpdateProducer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ITelegramBotClient _botClient;

        public UpdateProducer(IRabbitMqConnection connection, ITelegramBotClient botClient)
        {
            _connection = connection;
            _botClient = botClient;
        }

        public async Task Publish(Update update, string queueName)
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

            if (update.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
            {
                await _botClient.SendMessage(update.Message.Chat.Id, "Простите я работаю только в личных сообщениях.");
            }

            var connection = await _connection.GetConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var jsonUpdate = JsonSerializer.Serialize<Update>(update);
            var body = Encoding.UTF8.GetBytes(jsonUpdate);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
            Console.WriteLine($"[UpdateProducer] update was sent to {queueName} queue");
        }
    }
}

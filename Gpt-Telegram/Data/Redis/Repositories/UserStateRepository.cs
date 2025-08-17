using Gpt_Telegram.Data.Redis.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Gpt_Telegram.Data.Redis.Repositories
{
    public class UserStateRepository : IUserStateRepository
    {
        private readonly IDatabase _database;

        public UserStateRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task ClearStateAsync(long userId)
        {
            await _database.KeyDeleteAsync($"user:state:{userId}");
        }

        public async Task<UserState?> GetStateAsync(long userId)
        {
            var value = await _database.StringGetAsync($"user:state:{userId}");
            if (value.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<UserState>(value!);
        }

        public async Task UpdateStepAsync(long userId, string pipelineName, string stepName, Dictionary<string, object> data)
        {
            var state = new UserState
            {
                PipelineName = pipelineName,
                StepName = stepName,
                Data = data
            };

            var json = JsonSerializer.Serialize(state);

            await _database.StringSetAsync($"user:state:{userId}", json, TimeSpan.FromHours(1));
        }
    }
}

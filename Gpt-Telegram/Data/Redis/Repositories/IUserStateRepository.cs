using Gpt_Telegram.Data.Redis.Models;

namespace Gpt_Telegram.Data.Redis.Repositories
{
    public interface IUserStateRepository
    {
        Task<UserState?> GetStateAsync(long userId);
        Task UpdateStepAsync(long userId, string pipelineName, string stepName, Dictionary<string, object> data);
        Task ClearStateAsync(long userId);
    }
}

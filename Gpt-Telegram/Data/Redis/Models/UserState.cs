namespace Gpt_Telegram.Data.Redis.Models
{
    public class UserState
    {
        public string? PipelineName { get; set; }
        public string? StepName { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
}

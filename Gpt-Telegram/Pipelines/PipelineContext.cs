namespace Gpt_Telegram.Pipelines
{
    public class PipelineContext
    {
        public long UserId { get; set; }
        public string MessageText { get; set; }
        public string PipelineName { get; set; }
        public string StepName { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
}

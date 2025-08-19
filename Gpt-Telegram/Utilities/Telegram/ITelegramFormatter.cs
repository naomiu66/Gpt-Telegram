namespace Gpt_Telegram.Utilities.Telegram
{
    public interface ITelegramFormatter
    {
        public IEnumerable<string> FormatMessage(string message);
    }
}

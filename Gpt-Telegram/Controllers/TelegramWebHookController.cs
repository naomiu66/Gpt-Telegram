using Gpt_Telegram.Producer;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Gpt_Telegram.Controllers
{
    [Route("api/telegram")]
    [ApiController]
    public class TelegramWebHookController : ControllerBase
    {
        private readonly IUpdateProducer _updateProducer;

        public TelegramWebHookController(IUpdateProducer updateProducer)
        {
            _updateProducer = updateProducer;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message.Text.StartsWith("/"))
                {
                    await _updateProducer.Publish(update, "commands");
                    Console.WriteLine($"[Telegram] {update.Message.From.Username} : {update.Message.Text}");
                }
                else if (update.Type == UpdateType.Message)
                {
                    await _updateProducer.Publish(update, "prompts");
                    Console.WriteLine($"[Telegram] {update.Message.From.Username} : {update.Message.Text}");
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    //TODO
                }
                else
                    Console.WriteLine($"Unhandled update type: {update.Type}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling update: {ex.Message}");
            }

            return Ok();
        }
    }
}

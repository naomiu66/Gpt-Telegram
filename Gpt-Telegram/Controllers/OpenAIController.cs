using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace Gpt_Telegram.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly ChatClient _chatClient;

        public OpenAIController(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        [HttpPost]
        public async Task<IActionResult> ChatCompletion([FromBody] string message)
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(message);

            return Ok(new { response = completion.Content[0].Text });
        }
    }
}

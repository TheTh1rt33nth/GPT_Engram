using GPT_Engram.Services;
using Microsoft.AspNetCore.Mvc;

namespace GPT_Engram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly WarhammerChatService _chatService;

        public ChatController(WarhammerChatService chatService)
        {
            _chatService = chatService;
        }

        public class AskRequest
        {
            public string UserQuery { get; set; }
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return BadRequest("UserQuery is required.");
            }

            var answer = await _chatService.AskAsync(request.UserQuery);
            return Ok(new { answer });
        }
    }
}

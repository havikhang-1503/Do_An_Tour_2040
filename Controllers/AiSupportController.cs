using Microsoft.AspNetCore.Mvc;
using Tour_2040.Services;

namespace Tour_2040.Controllers
{
    public class UserRequest
    {
        public string Message { get; set; } = "";
    }

    public class AiSupportController : Controller
    {
        private readonly ISupportAiService _aiService;

        public AiSupportController(ISupportAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] UserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { answer = "Bạn chưa nhập nội dung." });
            }

            var answer = await _aiService.GetSupportAnswerAsync(request.Message);
            // Trả về JSON string nhận được từ AI
            return Ok(new { answer = answer });
        }
    }
}
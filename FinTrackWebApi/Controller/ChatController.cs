using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Services.ChatBotService; // IChatBotService için
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace FinTrackWebApi.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatBotService chatBotService, ILogger<ChatController> logger)
        {
            _chatBotService = chatBotService;
            _logger = logger;
        }

        private string GetCurrentUserIdString() // String olarak UserId döndür
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            var userId = GetCurrentUserIdString();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SendMessage: Kullanıcı kimliği alınamadı.");
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            if (!ModelState.IsValid) // DTO validasyonunu da ekleyelim
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("ChatController: SendMessage çağrıldı. UserId: {UserId}, ClientSessionId: {ClientSessionId}", userId, request.ClientChatSessionId);

            try
            {
                var responseDto = await _chatBotService.ProcessUserMessageAsync(request, userId);
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChatController: SendMessage işlenirken beklenmedik bir hata oluştu. UserId: {UserId}", userId);
                return StatusCode(500, new { error = "Mesajınız işlenirken sunucuda bir hata oluştu." });
            }
        }
    }
}
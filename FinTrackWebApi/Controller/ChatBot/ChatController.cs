using FinTrackWebApi.Dtos.ChatBotDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

[Authorize(Roles = "User,Admin")]
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ChatController> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private string GetCurrentUserIdString()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "null";
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
    {
        var userId = GetCurrentUserIdString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("Failed to verify user identity.");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var pythonChatBotUrl = _configuration["PythonChatBotService:Url"];
        if (string.IsNullOrWhiteSpace(pythonChatBotUrl))
        {
            _logger.LogError("PythonChatBotService:Url configuration is missing.");
            return StatusCode(500, "ChatBot service configuration error.");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = await HttpContext.GetTokenAsync("access_token");

            // Python servisine gönderilecek veri
            var payload = new
            {
                userId,
                clientChatSessionId = request.ClientChatSessionId,
                message = request.Message,
                authToken = token,
                // TODO: Gerekirse, basitleştirilmiş sohbet geçmişi de eklenebilir
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage responseFromPython = await httpClient.PostAsync(
                pythonChatBotUrl,
                content
            );

            if (responseFromPython.IsSuccessStatusCode)
            {
                var responseBody = await responseFromPython.Content.ReadAsStringAsync();
                var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                _logger.LogInformation(
                    "Received a reply from Python ChatBot service. UserId: {UserId}, SessionId: {SessionId}",
                    userId,
                    request.ClientChatSessionId
                );
                return Ok(chatResponse);
            }
            else
            {
                var errorBody = await responseFromPython.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Error response received from Python ChatBot service. Status: {StatusCode}, Body: {ErrorBody}, UserId: {UserId}",
                    responseFromPython.StatusCode,
                    errorBody,
                    userId
                );
                return StatusCode(
                    (int)responseFromPython.StatusCode,
                    new
                    {
                        error = "ChatBot servisinden beklenmeyen bir yanıt alındı.",
                        details = errorBody,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending request to Python ChatBot service. UserId: {UserId}",
                userId
            );
            return StatusCode(500, new { error = "Failed to contact the ChatBot service." });
        }
    }
}

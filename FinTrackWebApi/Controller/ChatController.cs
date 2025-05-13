// ChatController.cs (Yeni Yaklaşım)
using DocumentFormat.OpenXml.Spreadsheet;
using FinTrackWebApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ChatController> logger)
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
            return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var pythonChatBotUrl = _configuration["PythonChatBotService:Url"]; // appsettings.json'dan
        if (string.IsNullOrWhiteSpace(pythonChatBotUrl))
        {
            _logger.LogError("PythonChatBotService:Url yapılandırması eksik.");
            return StatusCode(500, "ChatBot servisi yapılandırma hatası.");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.GetTokenAsync("access_token").Result;

            // Python servisine gönderilecek veri
            var payload = new
            {
                userId = userId,
                clientChatSessionId = request.ClientChatSessionId,
                message = request.Message,
                authToken = token
                // Gerekirse, basitleştirilmiş sohbet geçmişi de eklenebilir
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Python ChatBot servisine istek gönderiliyor: {Url}, UserId: {UserId}, SessionId: {SessionId}",
                                   pythonChatBotUrl, userId, request.ClientChatSessionId);

            HttpResponseMessage responseFromPython = await httpClient.PostAsync(pythonChatBotUrl, content);

            if (responseFromPython.IsSuccessStatusCode)
            {
                var responseBody = await responseFromPython.Content.ReadAsStringAsync();
                // Python'dan gelen JSON yanıtını deserialize et (ChatResponseDto veya benzeri)
                var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("Python ChatBot servisinden yanıt alındı. UserId: {UserId}, SessionId: {SessionId}", userId, request.ClientChatSessionId);
                return Ok(chatResponse);
            }
            else
            {
                var errorBody = await responseFromPython.Content.ReadAsStringAsync();
                _logger.LogError("Python ChatBot servisinden hata yanıtı alındı. Status: {StatusCode}, Body: {ErrorBody}, UserId: {UserId}",
                                 responseFromPython.StatusCode, errorBody, userId);
                return StatusCode((int)responseFromPython.StatusCode, new { error = "ChatBot servisinden beklenmeyen bir yanıt alındı.", details = errorBody });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python ChatBot servisine istek gönderilirken hata oluştu. UserId: {UserId}", userId);
            return StatusCode(500, new { error = "ChatBot servisiyle iletişim kurulamadı." });
        }
    }
}
using FinTrackWebApi.Dtos;
using System.Text.Json;
using System.Text;

namespace FinTrackWebApi.Services.ChatBotService
{
    public class ChatBotService : IChatBotService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatBotService> _logger;

        public ChatBotService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ChatBotService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ChatResponseDto> SendMessageToPythonServiceAsync(ChatRequestDto request, string userId)
        {
            var pythonChatBotUrl = _configuration["PythonChatBotService:Url"];
            if (string.IsNullOrWhiteSpace(pythonChatBotUrl))
            {
                _logger.LogError("PythonChatBotService:Url yapılandırması eksik.");
                return new ChatResponseDto { Reply = "ChatBot servisi yapılandırma hatası." };
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("PythonChatBotClient"); // İsimlendirilmiş client kullanabiliriz

                var payload = new
                {
                    userId = userId,
                    clientChatSessionId = request.ClientChatSessionId,
                    message = request.Message
                };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogInformation("Python ChatBot servisine istek gönderiliyor: {Url}...", pythonChatBotUrl);
                HttpResponseMessage responseFromPython = await httpClient.PostAsync(pythonChatBotUrl, content);

                if (responseFromPython.IsSuccessStatusCode)
                {
                    var responseBody = await responseFromPython.Content.ReadAsStringAsync();
                    var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return chatResponse ?? new ChatResponseDto { Reply = "ChatBot'tan boş yanıt alındı." };
                }
                else
                {
                    var errorBody = await responseFromPython.Content.ReadAsStringAsync();
                    _logger.LogError("Python ChatBot servisinden hata yanıtı. Status: {StatusCode}, Body: {ErrorBody}",
                                     responseFromPython.StatusCode, errorBody);
                    // Kullanıcıya daha genel bir hata mesajı dönebiliriz
                    return new ChatResponseDto { Reply = "ChatBot ile iletişimde bir sorun oluştu." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Python ChatBot servisine istek gönderilirken hata oluştu.");
                return new ChatResponseDto { Reply = "ChatBot servisine ulaşılamadı." };
            }
        }
    }
}

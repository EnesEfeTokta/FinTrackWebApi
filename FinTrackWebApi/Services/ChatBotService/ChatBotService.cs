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
                _logger.LogError("PythonChatBotService:Url configuration is missing.");
                return new ChatResponseDto { Reply = "ChatBot service configuration error." };
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("PythonChatBotClient");

                var payload = new
                {
                    userId = userId,
                    clientChatSessionId = request.ClientChatSessionId,
                    message = request.Message
                };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending a request to the Python ChatBot service: {Url}...", pythonChatBotUrl);
                HttpResponseMessage responseFromPython = await httpClient.PostAsync(pythonChatBotUrl, content);

                if (responseFromPython.IsSuccessStatusCode)
                {
                    var responseBody = await responseFromPython.Content.ReadAsStringAsync();
                    var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return chatResponse ?? new ChatResponseDto { Reply = "Empty response received from ChatBot." };
                }
                else
                {
                    var errorBody = await responseFromPython.Content.ReadAsStringAsync();
                    _logger.LogError("Error response from Python ChatBot service. Status: {StatusCode}, Body: {ErrorBody}",
                                     responseFromPython.StatusCode, errorBody);
                    return new ChatResponseDto { Reply = "There was a problem communicating with the ChatBot." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request to Python ChatBot service.");
                return new ChatResponseDto { Reply = "ChatBot service could not be reached." };
            }
        }
    }
}

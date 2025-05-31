using FinTrackWebApi.Dtos;

namespace FinTrackWebApi.Services.ChatBotService
{
    public interface IChatBotService
    {
        Task<ChatResponseDto> SendMessageToPythonServiceAsync(ChatRequestDto request, string userId);
    }
}

using FinTrackWebApi.Controller;
using FinTrackWebApi.Dtos;
using System.Threading.Tasks;

namespace FinTrackWebApi.Services.ChatBotService
{
    public interface IChatBotService
    {
        Task<ChatResponseDto> ProcessUserMessageAsync(ChatRequestDto request, string userId);
    }
}
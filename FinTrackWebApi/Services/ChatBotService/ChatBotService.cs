using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Services.ChatBotService.Plugins;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text.Json;
using FinTrackWebApi.Controller;

namespace FinTrackWebApi.Services.ChatBotService
{
    public class ChatBotService : IChatBotService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ChatBotService> _logger;
        private const string ToolCallIdMetadataKey = "tool_call_id";

        public ChatBotService(
            Kernel kernel,
            IChatCompletionService chatCompletionService,
            IMemoryCache memoryCache,
            ILogger<ChatBotService> logger)
        {
            _kernel = kernel;
            _chatCompletionService = chatCompletionService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        private string GetSystemPrompt()
        {
            string availableFunctionDescriptions = "";
            if (_kernel.Plugins != null && _kernel.Plugins.Any())
            {
                List<string> descriptions = new List<string>();
                foreach (var plugin in _kernel.Plugins)
                {
                    foreach (var function in plugin)
                    {
                        if (!string.IsNullOrWhiteSpace(function.Description))
                        {
                            descriptions.Add($"'{plugin.Name}.{function.Name}': {function.Description.TrimEnd('.')}");
                        }
                        else
                        {
                            descriptions.Add($"'{plugin.Name}.{function.Name}' (Açıklama yok)");
                        }
                    }
                }
                if (descriptions.Any())
                {
                    availableFunctionDescriptions = "Kullanabileceğim bazı araçlar (fonksiyonlar) şunlardır: " + string.Join("; ", descriptions) + ". Bu fonksiyonları uygun durumlarda KENDİM ÇAĞIRACAĞIM ve sonuçlarını sana bildireceğim.";
                }
            }
            else
            {
                availableFunctionDescriptions = "Şu anda temel sohbet yeteneklerim var. Finansal araçlarım henüz yüklenmemiş.";
            }

            return $@"Sen FinBot'sun. FinTrack kullanıcılarına yardımcı olmak amacıyla geliştirilmiş bir yapay zeka sohbet botusun.
            Kullanıcılar sana 'sen kimsin?', 'amacın ne?', 'ne yapabilirsin?' gibi sorular sorduğunda, kendini 'Merhaba, ben FinBot. FinTrack kullanıcılarına yardımcı olmak amacıyla geliştirildim. {availableFunctionDescriptions}' şeklinde tanıt.
            Kullanıcının finansal işlemlerini sorgulayabilirsin. Bu işlemleri yapmak için sana sağlanan araçları (fonksiyonları) KENDİN ÇAĞIRMALISIN.
            LLM GÖREVİ: Kullanıcı isteğini analiz et. Eğer isteği karşılayacak bir fonksiyon varsa, o fonksiyonu uygun parametrelerle ÇAĞIR. Fonksiyon sonucunu al ve kullanıcıya anlamlı bir yanıt üret.
            Örneğin:
            - Kullanıcı 'tüm işlemlerimi göster' dediğinde, FinancePlugin.GetAllTransactionsAsync fonksiyonunu ÇAĞIR ve sonuçları kullanıcıya sun.
            - Kullanıcı 'sadece gelirlerimi göster' dediğinde, FinancePlugin.GetTransactionsByCategoryTypeAsync fonksiyonunu 'Gelir' parametresiyle ÇAĞIR.
            - Kullanıcı 'market harcamalarımı göster' dediğinde, FinancePlugin.GetTransactionsByCategoryNameAsync fonksiyonunu 'Market' parametresiyle ÇAĞIR.
            Dönen işlem listelerini kullanıcıya anlaşılır bir şekilde özetleyerek sun. Eğer fonksiyon bir hata döndürürse veya veri bulamazsa, bunu uygun bir şekilde kullanıcıya bildir.
            Eğer bir fonksiyonu çağırmak için gerekli bilgi eksikse (örneğin hangi kategori), bu bilgiyi kullanıcıdan İSTE. Yanıtlarında ASLA '(Bu kısımda ... fonksiyonu çağrılır)' gibi parantez içi açıklamalar KULLANMA; fonksiyonu gerçekten çağır ve sonucunu doğrudan ilet.
            Her zaman nazik ol. Finansal tavsiye verme. Karmaşık durumlar için uzmana yönlendir.
            Projenin adı FinTrack Finans Takip Uygulamasıdır.";
        }

        public async Task<ChatResponseDto> ProcessUserMessageAsync(ChatRequestDto request, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(request.ClientChatSessionId) || string.IsNullOrWhiteSpace(request.Message))
            {
                _logger.LogWarning("ProcessUserMessageAsync: Geçersiz parametreler. UserId: {UserId}, ClientChatSessionId: {ClientChatSessionId}, Message: {Message}",
                    userId, request.ClientChatSessionId, request.Message);
                return new ChatResponseDto { Reply = "İstek işlenirken bir sorun oluştu (eksik bilgi)." };
            }

            string chatCacheKey = $"ChatHistory_UserId_{userId}_SessionId_{request.ClientChatSessionId}";
            _logger.LogInformation("ProcessUserMessageAsync Başladı. CacheKey: {CacheKey}, Mesaj: {Message}", chatCacheKey, request.Message);

            try
            {
                if (!_memoryCache.TryGetValue(chatCacheKey, out ChatHistory chatHistory))
                {
                    chatHistory = new ChatHistory();
                    chatHistory.AddSystemMessage(GetSystemPrompt());
                    _logger.LogInformation("Yeni sohbet geçmişi oluşturuldu. CacheKey: {CacheKey}", chatCacheKey);
                }
                else
                {
                    _logger.LogInformation("Mevcut sohbet geçmişi yüklendi. HistoryCount: {Count}, CacheKey: {CacheKey}", chatHistory.Count, chatCacheKey);
                }

                chatHistory.AddUserMessage(request.Message);

#pragma warning disable SKEXP0070, SKEXP0071, SKEXP0072, SKEXP0020
                var geminiExecutionSettings = new GeminiPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false),
                    MaxTokens = 4096,
                    Temperature = 0.2,
                    TopP = 0.8
                };
#pragma warning restore SKEXP0070, SKEXP0071, SKEXP0072, SKEXP0020

                string finalAiReply;

                _logger.LogInformation("AI'a istek gönderiliyor (autoInvoke: true ile). History Count: {Count}, CacheKey: {CacheKey}", chatHistory.Count, chatCacheKey);
                ChatMessageContent aiResponse = await _chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings: geminiExecutionSettings,
                    kernel: _kernel
                );

                if (aiResponse == null)
                {
                    _logger.LogWarning("AI'dan null yanıt alındı. CacheKey: {CacheKey}", chatCacheKey);
                    finalAiReply = "Üzgünüm, şu an bir yanıt üretemiyorum.";
                }
                else if (!string.IsNullOrWhiteSpace(aiResponse.Content))
                {
                    finalAiReply = aiResponse.Content;
                    _logger.LogInformation("AI yanıt verdi (autoInvoke sonrası): {Reply}. CacheKey: {CacheKey}", finalAiReply, chatCacheKey);
                    if (aiResponse.Metadata != null && aiResponse.Metadata.TryGetValue("ToolCalls", out var toolCalls))
                    {
                        _logger.LogInformation("AI yanıtı, araç çağrıları (ToolCalls) metadata'sını içeriyor: {ToolCalls}. CacheKey: {CacheKey}", JsonSerializer.Serialize(toolCalls), chatCacheKey);
                    }
                    else if (aiResponse.Items != null && aiResponse.Items.OfType<FunctionCallContent>().Any())
                    {
                        _logger.LogInformation("AI yanıtı FunctionCallContent içeriyor (autoInvoke sonrası). CacheKey: {CacheKey}", chatCacheKey);
                    }
                }
                else
                {
                    _logger.LogWarning("AI'dan içeriksiz yanıt veya beklenmedik format (autoInvoke sonrası). CacheKey: {CacheKey}", chatCacheKey);
                    finalAiReply = "İsteğiniz işlendi ancak bir metin yanıtı oluşturulamadı. Lütfen farklı bir şekilde sormayı deneyin.";
                }

                chatHistory.AddAssistantMessage(finalAiReply);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _memoryCache.Set(chatCacheKey, chatHistory, cacheEntryOptions);

                _logger.LogInformation("Sohbet geçmişi güncellendi. AI Yanıtı: {AIReply}, CacheKey: {CacheKey}", finalAiReply, chatCacheKey);
                return new ChatResponseDto { Reply = finalAiReply };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessUserMessageAsync sırasında bir hata oluştu. CacheKey: {CacheKey}", chatCacheKey);
                return new ChatResponseDto { Reply = $"Bir hata oluştu ve isteğiniz işlenemedi: {ex.Message}" };
            }
        }
    }
}
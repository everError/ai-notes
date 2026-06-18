using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelAgent.Services;

public class CachingChatCompletionService(
    IChatCompletionService innerService, // 실제 AI 서비스 (e.g., OpenAIChatCompletionService)
    IMemoryCache cache) : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => innerService.Attributes;

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? promptExecutionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // 캐시 키로 사용할 마지막 사용자 메시지를 가져옵니다. (실제로는 더 정교한 키 생성 로직이 필요)
        string cacheKey = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content ?? string.Empty;

        // 1. 캐시에서 응답을 먼저 찾아봅니다.
        if (!string.IsNullOrEmpty(cacheKey) && cache.TryGetValue(cacheKey, out IReadOnlyList<ChatMessageContent>? cachedResult))
        {
            Console.WriteLine("[CACHE HIT] 캐시된 응답을 반환합니다.");
            return cachedResult!;
        }

        // 2. 캐시에 없으면, 실제 AI 서비스를 호출합니다.
        Console.WriteLine("[CACHE MISS] 실제 AI 서비스를 호출합니다.");
        var realResult = await innerService.GetChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel, cancellationToken);

        // 3. 결과를 캐시에 저장합니다. (예: 10분 동안)
        if (!string.IsNullOrEmpty(cacheKey))
        {
            cache.Set(cacheKey, realResult, TimeSpan.FromMinutes(10));
        }

        return realResult;
    }

    // 스트리밍 메서드 (이 예제에서는 캐싱 없이 바로 전달)
    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? promptExecutionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return innerService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel, cancellationToken);
    }
}
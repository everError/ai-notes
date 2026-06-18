## Semantic Kernel: `IChatCompletionService` 재정의를 통한 기능 확장 가이드

Semantic Kernel에서 `IChatCompletionService`는 AI 언어 모델과의 모든 채팅 기반 상호작용을 처리하는 핵심 인터페이스입니다. 이 인터페이스에 대한 자신만의 구현을 만들면, AI와의 모든 통신을 중간에서 가로채(Intercept) 원하는 모든 종류의 부가 기능을 추가할 수 있습니다.

이는 마치 AI 서비스에 나만의 '미들웨어(Middleware)'를 추가하는 것과 같으며, 일반적으로 \*\*데코레이터 패턴(Decorator Pattern)\*\*을 사용하여 기존 서비스의 기능을 감싸고 확장하는 방식으로 구현합니다.

---

### \#\# `IChatCompletionService` 재정의로 할 수 있는 일들

이 기법을 활용하면 애플리케이션의 요구사항에 맞춰 AI의 동작을 세밀하게 제어하고, 시스템의 안정성과 효율성을 크게 향상시킬 수 있습니다.

#### \#\#\# 1. 고급 로깅 및 모니터링 📝

AI와 주고받는 모든 프롬프트, 응답, 토큰 사용량, 응답 시간 등을 데이터베이스나 로그 파일에 기록하여 사용량을 분석하거나 문제를 추적할 수 있습니다.

#### \#\#\# 2. 응답 캐싱 (Response Caching) ⚡

동일한 질문이 반복적으로 들어올 때, 매번 비싼 LLM API를 호출하는 대신 캐시된 답변을 즉시 반환하여 비용과 응답 시간을 크게 줄입니다.

#### \#\#\# 3. 콘텐츠 필터링 및 개인정보 보호 🛡️

사용자의 프롬프트나 AI의 응답에 포함된 부적절한 내용이나 개인정보(PII)를 실시간으로 필터링하거나 마스킹하여 서비스의 안전성을 확보합니다.

#### \#\#\# 4. 모델 대체 및 장애 복구 (Model Fallback & Resilience) 🔄

주력 모델(예: GPT-4)에서 오류나 타임아웃이 발생했을 때, 자동으로 다른 모델(예: GPT-4o-mini)을 호출하도록 로직을 구현하여 서비스의 안정성을 높입니다.

#### \#\#\# 5. 단위 테스트를 위한 모의 객체 (Mocking for Unit Tests) 🧪

실제 API를 호출하지 않는 가짜(Mock) 서비스를 만들어, 특정 질문에 항상 정해진 답변을 반환하도록 설정할 수 있습니다. 이를 통해 AI와 무관한 비즈니스 로직을 안정적으로 테스트합니다.

#### \#\#\# 6. 미지원 모델 연결 (Connecting to Unsupported Models) 🔌

Semantic Kernel이 공식적으로 지원하지 않는 다른 LLM이나 내부적으로 구축한 모델이 있다면, 해당 모델의 API와 통신하는 `IChatCompletionService` 구현을 직접 만들어 프레임워크에 통합할 수 있습니다.

---

### \#\# 구현 예시: 캐싱 데코레이터

아래는 기존 `IChatCompletionService`에 캐싱 기능을 추가하는 데코레이터의 전체 코드 예시입니다.

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

/// <summary>
/// 기존 IChatCompletionService에 캐싱 기능을 덧씌우는 데코레이터 클래스
/// </summary>
public class CachingChatCompletionService(
    IChatCompletionService innerService, // 실제 AI 서비스
    IMemoryCache cache) : IChatCompletionService
{
    // --- Identity & Metadata Properties ---
    // 데코레이터는 원본 서비스의 정체성을 그대로 따라야 합니다.
    // 따라서 Attributes와 ModelId는 내부 서비스의 값을 그대로 반환해 줍니다.

    public IReadOnlyDictionary<string, object?> Attributes => innerService.Attributes;
    public string? ModelId => innerService.ModelId;

    // --- Execution Methods ---
    // 실제 동작하는 메서드에만 캐싱 로직을 추가합니다.

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? promptExecutionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content ?? string.Empty;

        if (!string.IsNullOrEmpty(cacheKey) && cache.TryGetValue(cacheKey, out IReadOnlyList<ChatMessageContent>? cachedResult))
        {
            Console.WriteLine("[CACHE HIT] 캐시된 응답을 반환합니다.");
            return cachedResult!;
        }

        Console.WriteLine("[CACHE MISS] 실제 AI 서비스를 호출합니다.");
        var realResult = await innerService.GetChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel, cancellationToken);

        if (!string.IsNullOrEmpty(cacheKey))
        {
            cache.Set(cacheKey, realResult, TimeSpan.FromMinutes(10));
        }

        return realResult;
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? promptExecutionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // 스트리밍은 캐싱이 복잡하므로 이 예제에서는 바로 전달합니다.
        return innerService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel, cancellationToken);
    }
}
```

---

### \#\# ASP.NET Core DI에 등록하는 방법

작성한 데코레이터 서비스를 ASP.NET Core의 의존성 주입(DI) 컨테이너에 등록하여 애플리케이션 전체에서 사용하도록 설정할 수 있습니다.

```csharp
// 캐시 서비스를 DI에 추가합니다.
builder.Services.AddMemoryCache();

// ▼▼▼ Kernel 자체를 모든 설정을 포함하여 싱글톤으로 등록합니다. ▼▼▼
builder.Services.AddSingleton(sp =>
{
    // 1. 실제 OpenAI 서비스를 먼저 생성합니다.
    //    (DI에 직접 등록하지 않고 KernelBuilder 내부에서만 사용)
    var innerService = new OpenAIChatCompletionService("gpt-4o-mini", "YOUR_API_KEY");

    // 2. 캐싱 데코레이터를 생성합니다.
    var cache = sp.GetRequiredService<IMemoryCache>();
    var cachingService = new CachingChatCompletionService(innerService, cache);

    // 3. KernelBuilder를 생성하고, 최종적으로 데코레이팅된 서비스를 추가합니다.
    var kernelBuilder = Kernel.CreateBuilder();

    // Kernel의 서비스 컬렉션에 직접 추가합니다.
    kernelBuilder.Services.AddSingleton<IChatCompletionService>(cachingService);

    // 4. 플러그인을 추가합니다.
    kernelBuilder.Plugins.AddFromType<BasicToolsPlugin>();

    // 5. 모든 설정이 완료된 Kernel을 빌드하여 반환합니다.
    return kernelBuilder.Build();
});
```


using Docs_Mcp.Ollama;
using Microsoft.Extensions.Options;

namespace Docs_Mcp.Services.Embedding;

public class OllamaEmbeddingService(HttpClient httpClient, IOptions<OllamaSettings> ollamaSettings) : IEmbeddingService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _modelName = ollamaSettings.Value.ModelName;
    public async Task<ReadOnlyMemory<float>> EmbedTextAsync(string text)
    {
        var request = new OllamaEmbeddingRequest(_modelName, text);

        // Ollama 임베딩 API 호출
        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
        response.EnsureSuccessStatusCode(); // HTTP 상태 코드 2xx가 아니면 예외 발생

        var embeddingResponse = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();

        if (embeddingResponse?.Embedding == null || embeddingResponse.Embedding.Count == 0)
        {
            throw new Exception("Ollama 임베딩 응답에 임베딩 벡터가 포함되어 있지 않습니다.");
        }

        return new ReadOnlyMemory<float>([.. embeddingResponse.Embedding]);
    }

    public async Task<List<ReadOnlyMemory<float>>> EmbedTextsAsync(List<string> texts)
    {
        // Ollama의 /api/embeddings 엔드포인트는 한 번에 하나의 프롬프트만 받습니다.
        // 따라서 여러 텍스트를 임베딩하려면 각 텍스트에 대해 개별적으로 API를 호출해야 합니다.
        // 병렬 처리를 통해 효율성을 높일 수 있습니다.
        var embeddingTasks = texts.Select(async text => await EmbedTextAsync(text)).ToList();
        return [.. (await Task.WhenAll(embeddingTasks))];
    }
}

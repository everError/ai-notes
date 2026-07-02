namespace Docs_Mcp.Services.Embedding;

public interface IEmbeddingService
{
    /// <summary>
    /// 단일 텍스트를 임베딩 벡터로 변환합니다.
    /// </summary>
    /// <param name="text">임베딩할 텍스트</param>
    /// <returns>텍스트의 임베딩 벡터</returns>
    Task<ReadOnlyMemory<float>> EmbedTextAsync(string text);

    /// <summary>
    /// 여러 텍스트를 임베딩 벡터 리스트로 변환합니다.
    /// </summary>
    /// <param name="texts">임베딩할 텍스트 리스트</param>
    /// <returns>각 텍스트의 임베딩 벡터 리스트</returns>
    Task<List<ReadOnlyMemory<float>>> EmbedTextsAsync(List<string> texts);
}
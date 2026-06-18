namespace Docs_Mcp.Ollama;

// Ollama API 요청/응답 모델 (필요에 따라 더 상세하게 정의 가능)
public record OllamaEmbeddingRequest(string Model, string Prompt, bool Stream = false);
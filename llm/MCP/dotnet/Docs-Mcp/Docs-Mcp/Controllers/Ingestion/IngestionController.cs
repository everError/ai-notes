using ChromaDB.Client;
using Docs_Mcp.Services.Document;
using Microsoft.AspNetCore.Mvc;

namespace Docs_Mcp.Controllers.Ingestion;

/// <summary>
/// IngestionController의 새 인스턴스를 초기화합니다.
/// DocumentIngestionService를 의존성 주입(DI) 컨테이너로부터 주입받습니다.
/// </summary>
/// <param name="ingestionService">문서 인제스트 및 검색 작업을 처리하는 서비스</param>
[ApiController] // 이 클래스가 API 컨트롤러임을 나타냅니다.
[Route("api/[controller]")] // 컨트롤러의 기본 라우트를 설정합니다. (예: /api/Ingestion)
public class IngestionController(DocumentIngestionService ingestionService, ChromaClient client) : ControllerBase
{
    private readonly DocumentIngestionService _ingestionService = ingestionService;
    private readonly ChromaClient _client = client;

    /// <summary>
    /// 프로젝트의 Data/Docs 폴더에 있는 Markdown 파일들을 읽어 ChromaDB에 임베딩하고 저장합니다.
    /// 이 작업은 idempotent(멱등)하게 설계되어 여러 번 호출해도 동일한 결과를 보장합니다.
    /// </summary>
    /// <returns>작업 성공 또는 실패 메시지</returns>
    [HttpPost("ingest-md-docs")] // HTTP POST 요청에 응답하며, 라우트는 /api/Ingestion/ingest-md-docs 입니다.
    public async Task<IActionResult> IngestMdDocuments()
    {
        try
        {
            var collection = await _client.GetOrCreateCollection("sample");
            //Console.WriteLine("[API 호출] 문서 인제스트 요청 수신.");
            //await _ingestionService.IngestMarkdownDocumentsAsync();
            return Ok("Markdown 문서 인제스트 및 임베딩 완료.");
        }
        catch (Exception ex)
        {
            // 예외 발생 시 오류 메시지를 콘솔에 출력하고 500 Internal Server Error를 반환합니다.
            Console.Error.WriteLine($"[API 오류] 문서 인제스트 중 오류 발생: {ex.Message}");
            return StatusCode(500, $"문서 인제스트 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// ChromaDB에 저장된 문서들에서 주어진 쿼리를 사용하여 유사한 문서를 검색합니다.
    /// </summary>
    /// <param name="query">검색할 텍스트 쿼리 (필수)</param>
    /// <param name="nResults">반환할 최대 결과 수 (기본값: 5)</param>
    /// <returns>검색된 문서들의 리스트 (ID, 내용, 메타데이터, 유사도 점수 포함)</returns>
    [HttpGet("search-docs")] // HTTP GET 요청에 응답하며, 라우트는 /api/Ingestion/search-docs 입니다.
    public async Task<IActionResult> SearchDocuments([FromQuery] string query, [FromQuery] int nResults = 5)
    {
        // 쿼리 문자열이 유효한지 확인합니다.
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("검색 쿼리는 필수입니다.");
        }

        // nResults 값이 유효한 범위 내에 있는지 확인합니다.
        if (nResults <= 0)
        {
            nResults = 5; // 유효하지 않은 경우 기본값으로 설정
        }

        try
        {
            Console.WriteLine($"[API 호출] 문서 검색 요청 수신. 쿼리: '{query}', 결과 수: {nResults}");
            var results = await _ingestionService.SearchDocumentsAsync(query, nResults);

            return Ok(results); // 검색 결과를 200 OK와 함께 반환합니다.
        }
        catch (Exception ex)
        {
            // 예외 발생 시 오류 메시지를 콘솔에 출력하고 500 Internal Server Error를 반환합니다.
            Console.Error.WriteLine($"[API 오류] 문서 검색 중 오류 발생: {ex.Message}");
            return StatusCode(500, $"문서 검색 중 오류 발생: {ex.Message}");
        }
    }
}

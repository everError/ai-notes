// Services/DocumentIngestionService.cs
using ChromaDB.Client;
using ChromaDB.Client.Models; // ChromaCollection 모델을 사용하기 위해 필요
using Microsoft.Extensions.Options; // IOptions<T> 사용을 위해 추가
using Docs_Mcp.Services.Embedding;
using Docs_Mcp.Data; // ChromaQueryInclude가 여기에 있을 가능성
// 또는


namespace Docs_Mcp.Services.Document;

public class DocumentIngestionService
{
    private readonly ChromaClient _chromaClient;
    private readonly IHostEnvironment _env;
    private readonly IEmbeddingService _embeddingService; // 주입될 임베딩 서비스 (OllamaEmbeddingService)
    private readonly ChromaConfigurationOptions _chromaConfigOptions; // 이제 생성자에서 직접 주입받음
    private readonly HttpClient _chromaHttpClient; // 이제 생성자에서 직접 주입받음

    private const string COLLECTION_NAME = "my_markdown_docs"; // ChromaDB에 사용할 컬렉션 이름

    /// <summary>
    /// DocumentIngestionService의 새 인스턴스를 초기화합니다.
    /// 필요한 의존성들을 의존성 주입(DI) 컨테이너로부터 주입받습니다.
    /// </summary>
    /// <param name="chromaClient">ChromaDB 서버와 통신하는 클라이언트 인스턴스</param>
    /// <param name="env">현재 호스팅 환경 정보 (예: ContentRootPath를 얻기 위함)</param>
    /// <param name="embeddingService">텍스트를 임베딩 벡터로 변환하는 서비스 (예: OllamaEmbeddingService)</param>
    /// <param name="chromaDbSettingsOptions">ChromaDB 연결 설정을 위한 IOptions 인스턴스</param>
    /// <param name="httpClientFactory">HttpClient 인스턴스를 생성하기 위한 팩토리</param>
    public DocumentIngestionService(
        ChromaClient chromaClient,
        IHostEnvironment env,
        IEmbeddingService embeddingService,
        IOptions<ChromaDbSettings> chromaDbSettingsOptions, // ChromaDbSettings 주입
        IHttpClientFactory httpClientFactory) // HttpClientFactory 주입
    {
        _chromaClient = chromaClient;
        _env = env;
        _embeddingService = embeddingService;

        // ChromaDbSettings에서 ChromaConfigurationOptions를 직접 생성합니다.
        var chromaDbSettings = chromaDbSettingsOptions.Value;
        _chromaConfigOptions = new ChromaConfigurationOptions(
            uri: new Uri(chromaDbSettings.GetBaseUri()),
            chromaToken: null // appsettings.json에서 ChromaToken을 설정하지 않았다면 null
        );

        // HttpClientFactory를 사용하여 HttpClient 인스턴스를 생성합니다.
        _chromaHttpClient = httpClientFactory.CreateClient();
        // HttpClient의 BaseAddress가 설정되지 않았다면 설정합니다.
        if (_chromaHttpClient.BaseAddress == null)
        {
            _chromaHttpClient.BaseAddress = new Uri(chromaDbSettings.GetBaseUri());
        }
    }

    /// <summary>
    /// 지정된 경로의 Markdown 파일들을 읽어 ChromaDB에 임베딩하고 저장합니다.
    /// ChromaDB 컬렉션이 없으면 새로 생성합니다.
    /// </summary>
    public async Task IngestMarkdownDocumentsAsync()
    {
        try { 
            // 1. ChromaClient를 통해 ChromaCollection 메타데이터를 가져오거나 생성합니다.
            //    이 메서드는 컬렉션의 ID, 이름, 메타데이터와 같은 정보를 담은 ChromaCollection 객체를 반환합니다.
            ChromaCollection collectionMetadata = await _chromaClient.GetOrCreateCollection(COLLECTION_NAME);

            // 2. 이 메타데이터와 ChromaClient의 내부 설정을 사용하여 ChromaCollectionClient 인스턴스를 생성합니다.
            //    ChromaCollectionClient는 실제 ChromaDB 컬렉션에 대한 데이터 추가, 조회, 업데이트 등의 작업을 수행하는 클라이언트입니다.
            ChromaCollectionClient collectionClient = new ChromaCollectionClient(
                collectionMetadata,
                _chromaConfigOptions, // 이제 직접 주입받은 _chromaConfigOptions 사용
                _chromaHttpClient     // 이제 직접 주입받은 _chromaHttpClient 사용
            );

            // MD 파일이 있는 'Data/Docs' 폴더의 절대 경로를 설정합니다.
            // _env.ContentRootPath는 프로젝트의 루트 디렉토리를 나타냅니다.
            var docsPath = Path.Combine(_env.ContentRootPath, "Data", "Docs");

            // Docs 폴더가 존재하는지 확인합니다.
            if (!Directory.Exists(docsPath))
            {
                Console.WriteLine($"[인제스트 오류] 지정된 경로를 찾을 수 없습니다: {docsPath}");
                return;
            }

            // 해당 폴더 내의 모든 .md 확장자 파일을 가져옵니다.
            var files = Directory.GetFiles(docsPath, "*.md");

            // 처리할 MD 파일이 없는 경우 메시지를 출력하고 종료합니다.
            if (files.Length == 0)
            {
                Console.WriteLine($"[인제스트] '{docsPath}' 경로에 처리할 MD 파일이 없습니다.");
                return;
            }

            var ids = new List<string>(); // ChromaDB에 저장될 문서의 고유 ID 리스트
            var documents = new List<string>(); // ChromaDB에 저장될 문서의 원본 텍스트 내용 리스트
            var metadatas = new List<Dictionary<string, object>>(); // ChromaDB에 저장될 문서의 메타데이터 리스트

            Console.WriteLine($"[인제스트 시작] '{docsPath}' 경로에서 MD 파일을 읽는 중...");

            // 각 MD 파일을 읽어 ID, 내용, 메타데이터를 준비합니다.
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath); // 파일명 (예: "rule1.md")
                var fileContent = await File.ReadAllTextAsync(filePath); // 파일의 전체 내용 읽기

                ids.Add(Path.GetFileNameWithoutExtension(fileName)); // 파일 확장자를 제외한 이름을 ID로 사용 (예: "rule1")
                documents.Add(fileContent); // 파일의 전체 내용을 문서로 저장
                metadatas.Add(new Dictionary<string, object>
                {
                    { "filename", fileName }, // 원본 파일명 저장
                    { "source_path", filePath }, // 원본 파일 경로 저장
                    { "ingested_at", DateTime.UtcNow.ToString("o") } // 인제스트된 UTC 시간 (ISO 8601 형식) 저장
                });

                Console.WriteLine($"[파일 읽음] {fileName}");
            }

            // 준비된 문서 데이터가 있다면 ChromaDB에 추가합니다.
            if (ids.Any())
            {
                // ChromaCollectionClient의 Add 메서드를 호출합니다.
                // 'documents' 매개변수에 텍스트를 제공하면, ChromaDB 서버가 내장 임베딩 모델을 사용하여
                // 해당 텍스트를 벡터로 자동으로 변환하여 저장합니다.
                await collectionClient.Add(ids: ids, documents: documents, metadatas: metadatas);
                Console.WriteLine($"[인제스트 완료] 총 {ids.Count}개의 문서가 '{COLLECTION_NAME}' 컬렉션에 성공적으로 임베딩 및 저장되었습니다.");
            }
            else
            {
                Console.WriteLine("[인제스트] 저장할 문서가 없어 작업을 건너뛰었습니다.");
            }
        }
        catch(Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// ChromaDB에 저장된 문서들에서 쿼리를 통해 유사한 문서를 검색합니다.
    /// 쿼리 텍스트는 주입된 임베딩 서비스 (_embeddingService, 즉 OllamaEmbeddingService)를 통해 벡터로 변환됩니다.
    /// </summary>
    /// <param name="query">검색할 텍스트 쿼리</param>
    /// <param name="nResults">반환할 최대 결과 수</param>
    /// <returns>검색된 문서들의 리스트 (ID, 내용, 메타데이터, 유사도 점수 포함)</returns>
    public async Task<object> SearchDocumentsAsync(string query, int nResults = 5)
    {
        // 1. ChromaClient를 통해 ChromaCollection 메타데이터를 가져옵니다.
        ChromaCollection collectionMetadata = await _chromaClient.GetOrCreateCollection(COLLECTION_NAME);

        // 2. 이 메타데이터와 ChromaClient의 내부 설정을 사용하여 ChromaCollectionClient 인스턴스를 생성합니다.
        ChromaCollectionClient collectionClient = new ChromaCollectionClient(
            collectionMetadata,
            _chromaConfigOptions, // 이제 직접 주입받은 _chromaConfigOptions 사용
            _chromaHttpClient     // 이제 직접 주입받은 _chromaHttpClient 사용
        );

        Console.WriteLine($"[검색 시작] 쿼리 텍스트: '{query}' (임베딩 서비스: {_embeddingService.GetType().Name} 사용)");

        // *** 핵심: 쿼리 텍스트를 임베딩 서비스(_embeddingService)를 통해 벡터로 변환합니다. ***
        // OllamaEmbeddingService는 이 텍스트를 Ollama API로 전송하여 임베딩 벡터를 받아옵니다.
        List<ReadOnlyMemory<float>> queryEmbeddings = new List<ReadOnlyMemory<float>>();
        queryEmbeddings.Add(await _embeddingService.EmbedTextAsync(query));

        // 이제 ChromaCollectionClient의 Query 메서드를 호출합니다.
        // 이 메서드는 임베딩된 쿼리 벡터만을 매개변수로 받습니다.
        // include 매개변수에 ChromaQueryInclude Enum의 조합을 전달합니다.
        var queryResults = await collectionClient.Query(
                  queryEmbeddings, // 'queryEmbeddings:' 명명된 인자 제거
                  nResults        // 'nResults:' 명명된 인자 제거
              );

        var results = new List<Dictionary<string, object>>();
        return results;
    }
}

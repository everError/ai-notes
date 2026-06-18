using ChromaDB.Client;
using Docs_Mcp.Data;
using Docs_Mcp.Ollama;
using Docs_Mcp.Services.Document;
using Docs_Mcp.Services.Embedding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMcpServer()
    .WithHttpTransport() // With streamable HTTP
    .WithToolsFromAssembly(); // Add all classes marked with [McpServerToolType]

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
//Ollama
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("OllamaSettings"));
// HttpClient 등록 (OllamaEmbeddingService에서 사용)
// Ollama BaseUrl을 HttpClient에 기본으로 설정합니다.
builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
{
    var ollamaSettings = builder.Configuration.GetSection("OllamaSettings").Get<OllamaSettings>();
    client.BaseAddress = new Uri(ollamaSettings!.BaseUrl);
});

// IEmbeddingService 인터페이스에 OllamaEmbeddingService 구현체를 등록합니다.
// AddHttpClient를 사용했으므로 자동으로 등록됩니다.
// builder.Services.AddSingleton<IEmbeddingService, OllamaEmbeddingService>(); // 이 줄은 필요 없음
//
//ChromaDB
builder.Services.Configure<ChromaDbSettings>(builder.Configuration.GetSection("ChromaDb"));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ChromaClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ChromaDbSettings>>().Value;
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();

    var configOptions = new ChromaConfigurationOptions(uri: settings.GetBaseUri());
    return new ChromaClient(configOptions, httpClient);
});
builder.Services.AddScoped<DocumentIngestionService>();
//
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapMcp("/mcp");
app.MapControllers();
app.UseCors();

app.Run();

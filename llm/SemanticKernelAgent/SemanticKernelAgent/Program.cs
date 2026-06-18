using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using SemanticKernelAgent.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.AddServiceDefaults();

var apiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI:ApiKey is required");
//
// 로거 설정
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// 캐시 서비스를 DI에 추가합니다.
builder.Services.AddMemoryCache();

builder.Services.AddSingleton(async sp =>
{
    // 1. 실제 OpenAI 서비스를 먼저 생성합니다.
    //    (DI에 직접 등록하지 않고 KernelBuilder 내부에서만 사용)
    var innerService = new OpenAIChatCompletionService("gpt-4o-mini", apiKey);

    // 2. 캐싱 데코레이터를 생성합니다.
    var cache = sp.GetRequiredService<IMemoryCache>();
    var cachingService = new CachingChatCompletionService(innerService, cache);

    // 3. KernelBuilder를 생성하고, 최종적으로 데코레이팅된 서비스를 추가합니다.
    var kernelBuilder = Kernel.CreateBuilder();

    // Kernel의 서비스 컬렉션에 직접 추가합니다.
    kernelBuilder.Services.AddSingleton<IChatCompletionService>(cachingService);
    // 4. MCP 서버에 연결
    var transport = new SseClientTransport(new()
    {
        Endpoint = new Uri("http://localhost:5298/mcp")
    });
    var mcpClient = await McpClientFactory.CreateAsync(transport);

    //// 5. MCP 서버의 모든 툴을 불러와서 Plugin으로 등록
    var tools = await mcpClient.ListToolsAsync();
    foreach (var tool in tools)
    {
        Console.WriteLine($"발견된 도구: {tool.Name}");
    }
    //kernelBuilder.Plugins.AddFromFunctions(
    //    "McpTools",
    //    tools.Select(tool => tool.AsKernelFunction())
    //);

    // 5. 모든 설정이 완료된 Kernel을 빌드하여 반환합니다.
    return kernelBuilder.Build();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.SemanticKernelAgent>("agent");
builder.AddProject<Projects.McpExample>("mcpexample");
builder.Build().Run();

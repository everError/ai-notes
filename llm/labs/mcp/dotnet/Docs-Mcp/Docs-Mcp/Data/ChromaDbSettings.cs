namespace Docs_Mcp.Data;

public class ChromaDbSettings
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? ApiVersion { get; set; }

    public string GetBaseUri()
    {
        return $"http://{Host}:{Port}/api/{ApiVersion}/";
    }
}

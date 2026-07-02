using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Docs_Mcp.Tools.Sample;

[McpServerToolType]
public class TimeTool
{
    [McpServerTool, Description("get current time")]
    public DateTime GetCurrentTime() => DateTime.Now;
}

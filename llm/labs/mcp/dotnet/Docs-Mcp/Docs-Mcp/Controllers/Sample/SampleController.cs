using Microsoft.AspNetCore.Mvc;

namespace Docs_Mcp.Controllers.Sample;

[ApiController]
[Route("/api/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    public string hello() => "Hello";
}

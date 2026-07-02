using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelAgent.Controllers;

[ApiController]
[Route("agent")]
public class AgentController(Kernel kernel) : ControllerBase
{
    private readonly Kernel _kernel = kernel;

    [HttpGet("ask")]
    public async Task<object> AskAsync(string prompt)
    {
        var history = new ChatHistory();

        history.AddUserMessage(prompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory: history,
            executionSettings,
            kernel: _kernel);

        return new
        {
            finalAnswer = result.Content, // result는 최종 텍스트 답변입니다.
            fullHistory = history
        };
    }
}

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SmartMillSync.Application.Plugins;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Agents;

public sealed class SemanticKernelChatGateway(
    Kernel kernel,
    IChatCompletionService chatCompletionService,
    MillTelemetryPlugin telemetryPlugin)
    : IIndustrialAgentChatGateway
{
    public async Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<AgentChatMessageDto> history,
        string message,
        CancellationToken cancellationToken)
    {
        var executionKernel = kernel.Clone();
        executionKernel.Plugins.AddFromObject(telemetryPlugin, "mill_telemetry");
        var chatHistory = new ChatHistory(systemPrompt);

        foreach (var historyMessage in history)
        {
            if (historyMessage.Role == AgentMessageRole.User)
            {
                chatHistory.AddUserMessage(historyMessage.Content);
            }
            else
            {
                chatHistory.AddAssistantMessage(historyMessage.Content);
            }
        }

        chatHistory.AddUserMessage(message);
        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var response = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            settings,
            executionKernel,
            cancellationToken);

        return string.IsNullOrWhiteSpace(response.Content)
            ? "Nao foi possivel gerar uma analise industrial conclusiva."
            : response.Content.Trim();
    }
}

using SmartMillSync.Application.Agents;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Services;

public sealed class UnavailableIndustrialAgentChatGateway : IIndustrialAgentChatGateway
{
    public Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<AgentChatMessageDto> history,
        string message,
        CancellationToken cancellationToken) =>
        throw new IndustrialAgentUnavailableException(
            "Google Gemini is not configured. Set GOOGLE_AI_API_KEY before using the industrial agent.");
}

public sealed class IndustrialAgentUnavailableException(string message) : Exception(message);

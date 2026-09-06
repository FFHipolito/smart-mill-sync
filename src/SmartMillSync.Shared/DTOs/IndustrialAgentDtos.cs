namespace SmartMillSync.Shared.DTOs;

public enum AgentMessageRole
{
    User = 1,
    Assistant = 2
}

public sealed record AgentChatMessageDto(
    AgentMessageRole Role,
    string Content);

public sealed record IndustrialAgentChatRequest(
    string Message,
    IReadOnlyList<AgentChatMessageDto> History);

public sealed record IndustrialAgentResponse(
    string Analysis,
    DateTimeOffset GeneratedAtUtc,
    string Model,
    Guid? DeliveryId = null);

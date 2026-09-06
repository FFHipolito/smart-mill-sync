using SmartMillSync.Application.Agents;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Agents;

public sealed class IndustrialAgentChatValidatorTests
{
    private readonly IndustrialAgentChatValidator _validator = new();

    [Fact]
    public async Task Validate_WithBoundedConversation_IsValid()
    {
        var request = new IndustrialAgentChatRequest(
            "Analise o patio.",
            [new AgentChatMessageDto(AgentMessageRole.User, "Qual e o risco?")]);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2001)]
    public async Task Validate_WithInvalidMessageLength_IsInvalid(int length)
    {
        var request = new IndustrialAgentChatRequest(new string('x', length), []);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Message));
    }

    [Fact]
    public async Task Validate_WithNullHistory_ReturnsValidationErrorWithoutThrowing()
    {
        var request = new IndustrialAgentChatRequest("Analyze", null!);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.History));
    }

    [Fact]
    public async Task Validate_WithTooManyHistoryMessages_IsInvalid()
    {
        var history = Enumerable.Range(0, 21)
            .Select(index => new AgentChatMessageDto(AgentMessageRole.User, $"Message {index}"))
            .ToArray();

        var result = await _validator.ValidateAsync(new IndustrialAgentChatRequest("Analyze", history));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(IndustrialAgentChatRequest.History));
    }

    [Fact]
    public async Task Validate_WithInvalidRoleOrOversizedHistoryContent_IsInvalid()
    {
        var history = new[]
        {
            new AgentChatMessageDto((AgentMessageRole)99, new string('x', 4_001))
        };

        var result = await _validator.ValidateAsync(new IndustrialAgentChatRequest("Analyze", history));

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(AgentChatMessageDto.Role)));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(AgentChatMessageDto.Content)));
    }
}

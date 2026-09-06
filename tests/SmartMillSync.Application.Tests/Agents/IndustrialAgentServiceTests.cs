using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Agents;

public sealed class IndustrialAgentServiceTests
{
    [Fact]
    public async Task ChatAsync_UsesPersonaTrimsHistoryAndReturnsConfiguredModel()
    {
        var gateway = Substitute.For<IIndustrialAgentChatGateway>();
        gateway.CompleteAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<AgentChatMessageDto>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("Analise operacional.");
        var service = CreateService(gateway, Substitute.For<ISender>());
        var history = Enumerable.Range(1, 25)
            .Select(index => new AgentChatMessageDto(AgentMessageRole.User, $"Message {index}"))
            .ToArray();

        var result = await service.ChatAsync(
            new IndustrialAgentChatRequest("  Avalie o patio  ", history),
            CancellationToken.None);

        Assert.Equal("Analise operacional.", result.Analysis);
        Assert.Equal("gemini-2.0-flash", result.Model);
        await gateway.Received(1).CompleteAsync(
            Arg.Is<string>(prompt => prompt.Contains("Oraculo Industrial da Suzano")),
            Arg.Is<IReadOnlyList<AgentChatMessageDto>>(messages => messages.Count == 20),
            "Avalie o patio",
            CancellationToken.None);
    }

    [Fact]
    public async Task DiagnoseDeliveryAsync_BuildsGroundedPromptForActiveDelivery()
    {
        var gateway = Substitute.For<IIndustrialAgentChatGateway>();
        gateway.CompleteAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<AgentChatMessageDto>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("Secagem natural recomendada.");
        var sender = Substitute.For<ISender>();
        var delivery = CreateDelivery();
        sender.Send(Arg.Any<GetActiveDeliveriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { delivery });
        var service = CreateService(gateway, sender);

        var result = await service.DiagnoseDeliveryAsync(delivery.Id, CancellationToken.None);

        Assert.Equal(delivery.Id, result.DeliveryId);
        await gateway.Received(1).CompleteAsync(
            Arg.Any<string>(),
            Arg.Is<IReadOnlyList<AgentChatMessageDto>>(messages => messages.Count == 0),
            Arg.Is<string>(prompt => prompt.Contains(delivery.TruckPlate) && prompt.Contains(delivery.Id.ToString())),
            CancellationToken.None);
    }

    [Fact]
    public async Task DiagnoseDeliveryAsync_WhenDeliveryIsNotActive_Throws()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetActiveDeliveriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<WoodDeliveryResponse>());
        var service = CreateService(Substitute.For<IIndustrialAgentChatGateway>(), sender);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.DiagnoseDeliveryAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public void Persona_RequiresTelemetryAndProhibitsFabrication()
    {
        Assert.Contains("ferramentas de telemetria", IndustrialAgentPersona.SystemPrompt);
        Assert.Contains("Nunca invente", IndustrialAgentPersona.SystemPrompt);
        Assert.Contains("prazos de secagem", IndustrialAgentPersona.SystemPrompt);
        Assert.Contains("sujeita a validacao do operador", IndustrialAgentPersona.SystemPrompt);
        Assert.Contains("pilhas de secagem natural", IndustrialAgentPersona.SystemPrompt);
    }

    private static IndustrialAgentService CreateService(
        IIndustrialAgentChatGateway gateway,
        ISender sender) => new(
        gateway,
        sender,
        Options.Create(new IndustrialAgentOptions { ModelId = "gemini-2.0-flash" }));

    private static WoodDeliveryResponse CreateDelivery() => new(
        Guid.NewGuid(), "ABC1D23", "Mucuri-04", "Eucalyptus", 50m, 10m, 40m,
        55m, 18m, DeliveryStatus.ArrivedAtGate, true, 770m, AlertLevel.High,
        DateTimeOffset.UtcNow);
}

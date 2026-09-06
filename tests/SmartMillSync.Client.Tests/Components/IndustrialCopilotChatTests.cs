using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartMillSync.Client.Components;
using SmartMillSync.Client.Services;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Client.Tests.Components;

public sealed class IndustrialCopilotChatTests : TestContext
{
    [Fact]
    public void Toggle_ExpandsOperationalChatPanel()
    {
        Services.AddSingleton(Substitute.For<IIndustrialAgentApiClient>());
        var component = RenderCopilot();

        component.Find(".copilot-toggle").Click();

        Assert.NotNull(component.Find("#industrial-copilot-panel"));
        Assert.Contains("Diagnostico energetico assistido", component.Markup);
    }

    [Fact]
    public async Task DiagnoseDeliveryAsync_RendersSafeMarkdownTable()
    {
        var agent = Substitute.For<IIndustrialAgentApiClient>();
        var delivery = CreateDelivery();
        agent.DiagnoseDeliveryAsync(delivery.Id, Arg.Any<CancellationToken>())
            .Returns(new IndustrialAgentResponse(
                "| Risco | Acao |\n|---|---|\n| **Alto** | Secagem |\n\n<script>alert(1)</script>",
                DateTimeOffset.UtcNow,
                "gemini-2.0-flash",
                delivery.Id));
        Services.AddSingleton(agent);
        var component = RenderCopilot();

        await component.Instance.DiagnoseDeliveryAsync(delivery);

        component.WaitForAssertion(() =>
        {
            Assert.NotNull(component.Find("table"));
            Assert.Contains("Secagem", component.Markup);
            Assert.DoesNotContain("<script>", component.Markup, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task DiagnoseDeliveryAsync_WhenGeminiIsNotConfigured_ShowsConfigurationMessage()
    {
        var agent = Substitute.For<IIndustrialAgentApiClient>();
        var delivery = CreateDelivery();
        agent.DiagnoseDeliveryAsync(delivery.Id, Arg.Any<CancellationToken>())
            .Returns<IndustrialAgentResponse>(_ => throw new IndustrialAgentApiException(
                "Unavailable",
                System.Net.HttpStatusCode.ServiceUnavailable));
        Services.AddSingleton(agent);
        var component = RenderCopilot();

        await component.Instance.DiagnoseDeliveryAsync(delivery);

        component.WaitForAssertion(() => Assert.Contains("GOOGLE_AI_API_KEY", component.Markup));
    }

    private IRenderedComponent<IndustrialCopilotChat> RenderCopilot()
    {
        RenderFragment markup = builder =>
        {
            builder.OpenComponent<IndustrialCopilotChat>(0);
            builder.CloseComponent();
        };
        return Render(markup).FindComponent<IndustrialCopilotChat>();
    }

    private static WoodDeliveryResponse CreateDelivery() => new(
        Guid.NewGuid(), "ABC1D23", "Mucuri-04", "Eucalyptus", 50m, 10m, 40m,
        55m, 18m, DeliveryStatus.ArrivedAtGate, true, 770m, AlertLevel.High,
        DateTimeOffset.UtcNow);
}

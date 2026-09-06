using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartMillSync.Client.Pages;
using SmartMillSync.Client.Services;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Client.Tests.Pages;

public sealed class YardOverviewTests : TestContext
{
    [Fact]
    public void Render_WithOperationalData_ShowsKpisDeliveryAndModal()
    {
        var api = Substitute.For<IMillApiClient>();
        var realtime = Substitute.For<IMillRealtimeClient>();
        var delivery = new WoodDeliveryResponse(
            Guid.NewGuid(), "ABC1D23", "Mucuri-04", "Eucalyptus Urograndis",
            50m, 10m, 40m, 55m, 18m, DeliveryStatus.ArrivedAtGate, true, 770m,
            AlertLevel.High, DateTimeOffset.UtcNow);
        api.GetActiveDeliveriesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { delivery });
        api.GetEnergyBalanceAsync(Arg.Any<CancellationToken>())
            .Returns(new EnergyBalanceSummaryDto(1, 40m, 18m, 770m, 55m, AlertLevel.High));
        Services.AddSingleton(api);
        Services.AddSingleton(realtime);
        JSInterop.Mode = JSRuntimeMode.Loose;
        RenderFragment pageMarkup = builder =>
        {
            builder.OpenComponent<YardOverview>(0);
            builder.CloseComponent();
        };

        var page = Render(pageMarkup).FindComponent<YardOverview>();
        page.WaitForAssertion(() =>
        {
            Assert.Contains("ABC1D23", page.Markup);
            Assert.Contains("Mucuri-04", page.Markup);
            Assert.Contains("770", page.Markup);
            Assert.Contains("alert-high", page.Markup);
        });

        page.Find("button.primary-action").Click();

        Assert.NotNull(page.Find("[role='dialog']"));
        Assert.Contains("Confirmar chegada", page.Markup);
    }

    [Fact]
    public void Render_WhenApiFails_ShowsActionableError()
    {
        var api = Substitute.For<IMillApiClient>();
        var realtime = Substitute.For<IMillRealtimeClient>();
        api.GetActiveDeliveriesAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<WoodDeliveryResponse>>(_ => throw new HttpRequestException());
        Services.AddSingleton(api);
        Services.AddSingleton(realtime);
        RenderFragment pageMarkup = builder =>
        {
            builder.OpenComponent<YardOverview>(0);
            builder.CloseComponent();
        };

        var page = Render(pageMarkup).FindComponent<YardOverview>();

        page.WaitForAssertion(() => Assert.Contains("Verifique a API industrial", page.Markup));
    }
}

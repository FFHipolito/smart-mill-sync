using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using NSubstitute;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Application.Plugins;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Plugins;

public sealed class MillTelemetryPluginTests
{
    [Fact]
    public async Task GetCurrentEnergyBalanceAsync_ReturnsQueryResult()
    {
        var sender = Substitute.For<ISender>();
        var expected = new EnergyBalanceSummaryDto(2, 80m, 40m, 770m, 50m, AlertLevel.High);
        sender.Send(Arg.Any<GetMillEnergyBalanceQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
        var plugin = CreatePlugin(sender);

        var result = await plugin.GetCurrentEnergyBalanceAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AnalyzeMoistureImpact_CalculatesGasAndConfiguredCost()
    {
        var plugin = CreatePlugin(Substitute.For<ISender>());

        var result = plugin.AnalyzeMoistureImpact(55m, 40m);

        Assert.Equal(770m, result.ExtraGasVolumeNm3);
        Assert.Equal(1_925m, result.EstimatedCost);
    }

    [Fact]
    public async Task GetHighMoistureDeliveriesAsync_FiltersAndOrdersActiveLoads()
    {
        var sender = Substitute.For<ISender>();
        var normal = CreateDelivery(Guid.NewGuid(), 45m);
        var humid = CreateDelivery(Guid.NewGuid(), 55m);
        var critical = CreateDelivery(Guid.NewGuid(), 60m);
        sender.Send(Arg.Any<GetActiveDeliveriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] { normal, humid, critical });
        var plugin = CreatePlugin(sender);

        var result = await plugin.GetHighMoistureDeliveriesAsync();

        Assert.Equal(new[] { critical.Id, humid.Id }, result.Select(delivery => delivery.Id));
    }

    [Fact]
    public void PublicTools_AreDecoratedAsKernelFunctions()
    {
        var kernelFunctions = typeof(MillTelemetryPlugin)
            .GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(KernelFunctionAttribute), false).Length > 0)
            .ToArray();

        Assert.Equal(3, kernelFunctions.Length);
    }

    private static MillTelemetryPlugin CreatePlugin(ISender sender) => new(
        sender,
        Options.Create(new IndustrialAgentOptions { GasPricePerNm3 = 2.50m }));

    private static WoodDeliveryResponse CreateDelivery(Guid id, decimal moisture) => new(
        id, "ABC1D23", "Origin", "Species", 50m, 10m, 40m, moisture,
        40m * (1m - moisture / 100m), DeliveryStatus.ArrivedAtGate,
        moisture > 50m, moisture > 50m ? (moisture - 50m) * 3.85m * 40m : 0m,
        moisture >= 55m ? AlertLevel.High : AlertLevel.Normal, DateTimeOffset.UtcNow);
}

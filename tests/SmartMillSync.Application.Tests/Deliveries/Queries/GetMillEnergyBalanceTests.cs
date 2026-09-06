using NSubstitute;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Deliveries.Queries;

public sealed class GetMillEnergyBalanceTests
{
    [Fact]
    public async Task Handle_AggregatesDailyMetricsAndClassifiesAlert()
    {
        var repository = Substitute.For<IWoodDeliveryRepository>();
        var normal = new WoodDelivery("ABC1D23", "A", "Eucalyptus", 50m, 10m, 45m);
        var humid = new WoodDelivery("DEF4G56", "B", "Eucalyptus", 50m, 10m, 55m);
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { normal, humid });
        var handler = new GetMillEnergyBalanceQueryHandler(repository);

        var result = await handler.Handle(new GetMillEnergyBalanceQuery(), CancellationToken.None);

        Assert.Equal(2, result.TotalDeliveries);
        Assert.Equal(80m, result.TotalNetWeightTons);
        Assert.Equal(50m, result.AverageMoisturePercentage);
        Assert.Equal(770m, result.EstimatedAdditionalGasNm3);
        Assert.Equal(AlertLevel.High, result.AlertLevel);
    }

    [Fact]
    public async Task Handle_WithNoDeliveries_ReturnsZeroedNormalSummary()
    {
        var repository = Substitute.For<IWoodDeliveryRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<WoodDelivery>());
        var handler = new GetMillEnergyBalanceQueryHandler(repository);

        var result = await handler.Handle(new GetMillEnergyBalanceQuery(), CancellationToken.None);

        Assert.Equal(0, result.TotalDeliveries);
        Assert.Equal(0m, result.AverageMoisturePercentage);
        Assert.Equal(AlertLevel.Normal, result.AlertLevel);
    }
}

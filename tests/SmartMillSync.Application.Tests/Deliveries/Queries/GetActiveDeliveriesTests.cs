using NSubstitute;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Domain.Entities;
using Xunit;

namespace SmartMillSync.Application.Tests.Deliveries.Queries;

public sealed class GetActiveDeliveriesTests
{
    [Fact]
    public async Task Handle_MapsAndOrdersActiveDeliveriesByArrivalDescending()
    {
        var repository = Substitute.For<IWoodDeliveryRepository>();
        var first = new WoodDelivery("ABC1D23", "A", "Eucalyptus", 40m, 10m, 45m);
        var second = new WoodDelivery("DEF4G56", "B", "Eucalyptus", 50m, 10m, 55m);
        repository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { first, second });
        var handler = new GetActiveDeliveriesQueryHandler(repository);

        var result = await handler.Handle(new GetActiveDeliveriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(second.Id, result[0].Id);
        Assert.Equal(first.Id, result[1].Id);
    }
}

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SmartMillSync.Api.Controllers;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Api.Tests.Controllers;

public sealed class WoodDeliveriesControllerTests
{
    [Fact]
    public async Task Register_DispatchesCommandAndReturnsCreated()
    {
        var sender = Substitute.For<ISender>();
        var request = new CreateWoodDeliveryRequest("ABC1D23", "Origin", "Species", 50m, 10m, 45m);
        var response = CreateDeliveryResponse();
        sender.Send(Arg.Any<RegisterWoodDeliveryCommand>(), Arg.Any<CancellationToken>())
            .Returns(response);
        var controller = new WoodDeliveriesController(sender);

        var result = await controller.Register(request, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(response, created.Value);
        await sender.Received(1).Send(
            Arg.Is<RegisterWoodDeliveryCommand>(command => command.Request == request),
            CancellationToken.None);
    }

    [Fact]
    public async Task GetActive_DispatchesQueryAndReturnsOk()
    {
        var sender = Substitute.For<ISender>();
        IReadOnlyList<WoodDeliveryResponse> deliveries = new[] { CreateDeliveryResponse() };
        sender.Send(Arg.Any<GetActiveDeliveriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(deliveries);
        var controller = new WoodDeliveriesController(sender);

        var result = await controller.GetActive(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(deliveries, ok.Value);
    }

    [Fact]
    public async Task GetEnergyBalance_DispatchesQueryAndReturnsOk()
    {
        var sender = Substitute.For<ISender>();
        var summary = new EnergyBalanceSummaryDto(1, 40m, 22m, 0m, 45m, AlertLevel.Normal);
        sender.Send(Arg.Any<GetMillEnergyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(summary);
        var controller = new WoodDeliveriesController(sender);

        var result = await controller.GetEnergyBalance(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(summary, ok.Value);
    }

    private static WoodDeliveryResponse CreateDeliveryResponse() => new(
        Guid.NewGuid(), "ABC1D23", "Origin", "Species", 50m, 10m, 40m, 45m,
        22m, DeliveryStatus.ArrivedAtGate, false, 0m, AlertLevel.Normal,
        DateTimeOffset.UtcNow);
}

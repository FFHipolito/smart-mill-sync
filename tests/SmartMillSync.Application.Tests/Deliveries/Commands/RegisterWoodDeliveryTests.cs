using NSubstitute;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Deliveries.Commands;

public sealed class RegisterWoodDeliveryTests
{
    [Fact]
    public async Task Handle_PersistsMapsAndNotifiesDelivery()
    {
        var repository = Substitute.For<IWoodDeliveryRepository>();
        var notifications = Substitute.For<ISignalRNotificationService>();
        var handler = new RegisterWoodDeliveryCommandHandler(repository, notifications);
        var request = new CreateWoodDeliveryRequest("abc-1d23", "Mucuri", "Urograndis", 50m, 10m, 55m);

        var response = await handler.Handle(new RegisterWoodDeliveryCommand(request), CancellationToken.None);

        Assert.Equal("ABC1D23", response.TruckPlate);
        Assert.Equal(770m, response.ExtraGasVolume);
        Assert.Equal(AlertLevel.High, response.AlertLevel);
        await repository.Received(1).AddAsync(Arg.Any<WoodDelivery>(), CancellationToken.None);
        await notifications.Received(1).NotifyDeliveryUpdatedAsync(response, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenPersistenceFails_DoesNotNotify()
    {
        var repository = Substitute.For<IWoodDeliveryRepository>();
        var notifications = Substitute.For<ISignalRNotificationService>();
        repository.AddAsync(Arg.Any<WoodDelivery>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Persistence failed."));
        var handler = new RegisterWoodDeliveryCommandHandler(repository, notifications);
        var command = new RegisterWoodDeliveryCommand(
            new CreateWoodDeliveryRequest("ABC1D23", "Mucuri", "Urograndis", 50m, 10m, 45m));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        await notifications.DidNotReceiveWithAnyArgs().NotifyDeliveryUpdatedAsync(default!, default);
    }
}

using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using SmartMillSync.Api.Hubs;
using SmartMillSync.Api.Services;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Api.Tests.Services;

public sealed class SignalRNotificationServiceTests
{
    [Fact]
    public async Task Notifications_AreBroadcastToAllTypedClients()
    {
        var client = Substitute.For<IMillSyncClient>();
        var clients = Substitute.For<IHubClients<IMillSyncClient>>();
        clients.All.Returns(client);
        var context = Substitute.For<IHubContext<MillSyncHub, IMillSyncClient>>();
        context.Clients.Returns(clients);
        var service = new SignalRNotificationService(context);
        var delivery = new WoodDeliveryResponse(
            Guid.NewGuid(), "ABC1D23", "Origin", "Species", 50m, 10m, 40m,
            55m, 18m, DeliveryStatus.ArrivedAtGate, true, 770m, AlertLevel.High,
            DateTimeOffset.UtcNow);
        var summary = new EnergyBalanceSummaryDto(1, 40m, 18m, 770m, 55m, AlertLevel.High);

        await service.NotifyDeliveryUpdatedAsync(delivery, CancellationToken.None);
        await service.NotifyEnergyBalanceAsync(summary, CancellationToken.None);

        await client.Received(1).DeliveryUpdated(delivery);
        await client.Received(1).EnergyBalanceUpdated(summary);
    }
}

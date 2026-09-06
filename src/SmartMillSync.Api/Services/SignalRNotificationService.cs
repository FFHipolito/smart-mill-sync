using Microsoft.AspNetCore.SignalR;
using SmartMillSync.Api.Hubs;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Services;

public sealed class SignalRNotificationService(
    IHubContext<MillSyncHub, IMillSyncClient> hubContext)
    : ISignalRNotificationService
{
    public Task NotifyDeliveryUpdatedAsync(
        WoodDeliveryResponse delivery,
        CancellationToken cancellationToken) =>
        hubContext.Clients.All.DeliveryUpdated(delivery);

    public Task NotifyEnergyBalanceAsync(
        EnergyBalanceSummaryDto summary,
        CancellationToken cancellationToken) =>
        hubContext.Clients.All.EnergyBalanceUpdated(summary);
}

using Microsoft.AspNetCore.SignalR;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Hubs;

public interface IMillSyncClient
{
    Task DeliveryUpdated(WoodDeliveryResponse delivery);
    Task EnergyBalanceUpdated(EnergyBalanceSummaryDto summary);
}

public sealed class MillSyncHub : Hub<IMillSyncClient>;

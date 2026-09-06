using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Abstractions;

public interface ISignalRNotificationService
{
    Task NotifyDeliveryUpdatedAsync(WoodDeliveryResponse delivery, CancellationToken cancellationToken);
    Task NotifyEnergyBalanceAsync(EnergyBalanceSummaryDto summary, CancellationToken cancellationToken);
}

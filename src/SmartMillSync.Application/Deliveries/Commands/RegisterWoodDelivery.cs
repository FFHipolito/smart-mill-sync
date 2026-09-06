using MediatR;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Shared.DTOs;
using SharedAlertLevel = SmartMillSync.Shared.DTOs.AlertLevel;
using SharedDeliveryStatus = SmartMillSync.Shared.DTOs.DeliveryStatus;

namespace SmartMillSync.Application.Deliveries.Commands;

public sealed record RegisterWoodDeliveryCommand(CreateWoodDeliveryRequest Request)
    : IRequest<WoodDeliveryResponse>;

public sealed class RegisterWoodDeliveryCommandHandler(
    IWoodDeliveryRepository repository,
    ISignalRNotificationService notificationService)
    : IRequestHandler<RegisterWoodDeliveryCommand, WoodDeliveryResponse>
{
    public async Task<WoodDeliveryResponse> Handle(
        RegisterWoodDeliveryCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command.Request);

        var request = command.Request;
        var delivery = new WoodDelivery(
            request.TruckPlate,
            request.ForestOrigin,
            request.WoodSpecies,
            request.GrossWeight,
            request.TareWeight,
            request.MoisturePercentage);

        await repository.AddAsync(delivery, cancellationToken);

        var response = Map(delivery);
        await notificationService.NotifyDeliveryUpdatedAsync(response, cancellationToken);
        return response;
    }

    internal static WoodDeliveryResponse Map(WoodDelivery delivery) => new(
        delivery.Id,
        delivery.TruckPlate,
        delivery.ForestOrigin,
        delivery.WoodSpecies,
        delivery.GrossWeight,
        delivery.TareWeight,
        delivery.NetWeight,
        delivery.MoisturePercentage,
        delivery.DryWeightTons,
        (SharedDeliveryStatus)(int)delivery.Status,
        delivery.RequiresThermalCompensation,
        delivery.ExtraGasVolume,
        delivery.HasHighGasAlert
            ? SharedAlertLevel.High
            : delivery.RequiresThermalCompensation
                ? SharedAlertLevel.Moderate
                : SharedAlertLevel.Normal,
        delivery.ArrivedAtUtc);
}

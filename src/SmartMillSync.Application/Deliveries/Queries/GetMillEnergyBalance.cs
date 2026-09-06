using MediatR;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Deliveries.Queries;

public sealed record GetMillEnergyBalanceQuery : IRequest<EnergyBalanceSummaryDto>;

public sealed class GetMillEnergyBalanceQueryHandler(IWoodDeliveryRepository repository)
    : IRequestHandler<GetMillEnergyBalanceQuery, EnergyBalanceSummaryDto>
{
    public async Task<EnergyBalanceSummaryDto> Handle(
        GetMillEnergyBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var deliveries = await repository.GetAllAsync(cancellationToken);
        var today = DateTimeOffset.UtcNow.Date;
        var dailyDeliveries = deliveries
            .Where(delivery => delivery.ArrivedAtUtc.UtcDateTime.Date == today)
            .ToArray();

        var additionalGas = dailyDeliveries.Sum(delivery => delivery.ExtraGasVolume);
        var alertLevel = additionalGas > 150m
            ? AlertLevel.High
            : additionalGas > 0m
                ? AlertLevel.Moderate
                : AlertLevel.Normal;

        return new EnergyBalanceSummaryDto(
            dailyDeliveries.Length,
            dailyDeliveries.Sum(delivery => delivery.NetWeight),
            dailyDeliveries.Sum(delivery => delivery.DryWeightTons),
            additionalGas,
            dailyDeliveries.Length == 0
                ? 0m
                : dailyDeliveries.Average(delivery => delivery.MoisturePercentage),
            alertLevel);
    }
}

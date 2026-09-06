using MediatR;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Deliveries.Queries;

public sealed record GetActiveDeliveriesQuery : IRequest<IReadOnlyList<WoodDeliveryResponse>>;

public sealed class GetActiveDeliveriesQueryHandler(IWoodDeliveryRepository repository)
    : IRequestHandler<GetActiveDeliveriesQuery, IReadOnlyList<WoodDeliveryResponse>>
{
    public async Task<IReadOnlyList<WoodDeliveryResponse>> Handle(
        GetActiveDeliveriesQuery request,
        CancellationToken cancellationToken)
    {
        var deliveries = await repository.GetActiveAsync(cancellationToken);
        return deliveries
            .OrderByDescending(delivery => delivery.ArrivedAtUtc)
            .Select(RegisterWoodDeliveryCommandHandler.Map)
            .ToArray();
    }
}

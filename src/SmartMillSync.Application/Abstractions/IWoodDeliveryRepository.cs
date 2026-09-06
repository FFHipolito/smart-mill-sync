using SmartMillSync.Domain.Entities;

namespace SmartMillSync.Application.Abstractions;

public interface IWoodDeliveryRepository
{
    Task AddAsync(WoodDelivery delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<WoodDelivery>> GetActiveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<WoodDelivery>> GetAllAsync(CancellationToken cancellationToken);
}

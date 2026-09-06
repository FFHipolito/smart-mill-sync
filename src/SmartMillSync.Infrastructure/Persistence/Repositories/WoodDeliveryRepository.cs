using Microsoft.EntityFrameworkCore;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Domain.Enums;

namespace SmartMillSync.Infrastructure.Persistence.Repositories;

public sealed class WoodDeliveryRepository(SmartMillDbContext dbContext)
    : IWoodDeliveryRepository
{
    public async Task AddAsync(WoodDelivery delivery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        await dbContext.WoodDeliveries.AddAsync(delivery, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WoodDelivery>> GetActiveAsync(
        CancellationToken cancellationToken) =>
        await dbContext.WoodDeliveries
            .AsNoTracking()
            .Where(delivery =>
                delivery.Status != DeliveryStatus.Completed &&
                delivery.Status != DeliveryStatus.Rejected)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WoodDelivery>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.WoodDeliveries
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}

using SmartMillSync.Domain.Entities;
using SmartMillSync.Domain.Enums;
using SmartMillSync.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Persistence;

public sealed class WoodDeliveryRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsDelivery()
    {
        await using var context = SmartMillDbContextTests.CreateContext();
        var repository = new WoodDeliveryRepository(context);
        var delivery = CreateDelivery("ABC1D23");

        await repository.AddAsync(delivery, CancellationToken.None);

        Assert.Single(await repository.GetAllAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesCompletedAndRejectedDeliveries()
    {
        await using var context = SmartMillDbContextTests.CreateContext();
        var active = CreateDelivery("ABC1D23");
        var completed = CreateDelivery("DEF4G56");
        var rejected = CreateDelivery("GHI7J89");
        completed.ChangeStatus(DeliveryStatus.Completed);
        rejected.ChangeStatus(DeliveryStatus.Rejected);
        context.WoodDeliveries.AddRange(active, completed, rejected);
        await context.SaveChangesAsync();
        var repository = new WoodDeliveryRepository(context);

        var result = await repository.GetActiveAsync(CancellationToken.None);

        Assert.Collection(result, delivery => Assert.Equal(active.Id, delivery.Id));
    }

    private static WoodDelivery CreateDelivery(string plate) =>
        new(plate, "Origin", "Species", 50m, 10m, 45m);
}

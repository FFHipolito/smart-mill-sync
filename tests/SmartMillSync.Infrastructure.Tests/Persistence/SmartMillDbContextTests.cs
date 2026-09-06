using Microsoft.EntityFrameworkCore;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Infrastructure.Persistence;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Persistence;

public sealed class SmartMillDbContextTests
{
    [Fact]
    public void Context_ExposesExpectedEntitySets()
    {
        using var context = CreateContext();

        Assert.IsAssignableFrom<DbSet<WoodDelivery>>(context.WoodDeliveries);
        Assert.IsAssignableFrom<DbSet<GasTelemetryRecord>>(context.GasTelemetryRecords);
    }

    internal static SmartMillDbContext CreateContext() => new(
        new DbContextOptionsBuilder<SmartMillDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartMillSync.Api.Health;
using SmartMillSync.Infrastructure.Persistence;
using Xunit;

namespace SmartMillSync.Api.Tests.Health;

public sealed class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithReachableInMemoryDatabase_IsHealthy()
    {
        await using var context = new SmartMillDbContext(
            new DbContextOptionsBuilder<SmartMillDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        var healthCheck = new DatabaseHealthCheck(context);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}

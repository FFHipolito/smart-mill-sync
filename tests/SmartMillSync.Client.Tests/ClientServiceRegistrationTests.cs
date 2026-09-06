using Microsoft.Extensions.DependencyInjection;
using SmartMillSync.Client.Services;
using Xunit;

namespace SmartMillSync.Client.Tests;

public sealed class ClientServiceRegistrationTests
{
    [Fact]
    public async Task AddSmartMillClient_RegistersTypedClientsWithConfiguredBaseAddress()
    {
        var services = new ServiceCollection();
        services.AddSmartMillClient(new Uri("http://localhost:5243/"));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        Assert.Equal(
            new Uri("http://localhost:5243/"),
            scope.ServiceProvider.GetRequiredService<HttpClient>().BaseAddress);
        Assert.NotNull(scope.ServiceProvider.GetService<MillApiClient>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMillApiClient>());
        Assert.NotNull(scope.ServiceProvider.GetService<MillRealtimeClient>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMillRealtimeClient>());
        Assert.NotNull(scope.ServiceProvider.GetService<IndustrialAgentApiClient>());
        Assert.NotNull(scope.ServiceProvider.GetService<IIndustrialAgentApiClient>());
    }
}

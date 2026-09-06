using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartMillSync.Api.Hubs;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Agents;
using SmartMillSync.Infrastructure.Persistence;
using Xunit;

namespace SmartMillSync.Api.Tests;

public sealed class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public void Host_RegistersCoreApplicationServices()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ISender>());
        Assert.NotNull(scope.ServiceProvider.GetService<IWoodDeliveryRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISignalRNotificationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IIndustrialAgentService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IIndustrialAgentChatGateway>());
        Assert.NotNull(scope.ServiceProvider.GetService<SmartMillDbContext>());
    }

    [Fact]
    public async Task Host_MapsSignalRHubEndpoint()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/hubs/mill-sync");

        Assert.NotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(typeof(Microsoft.AspNetCore.SignalR.Hub).IsAssignableFrom(typeof(MillSyncHub)));
    }

    [Fact]
    public void Configuration_UsesPostgreSqlProvider()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartMillDbContext>();

        Assert.Contains("Npgsql", context.Database.ProviderName);
    }
}

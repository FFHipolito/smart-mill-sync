using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmartMillSync.Api.Tests.Security;

[Collection("Environment variables")]
public sealed class ProductionSecurityIntegrationTests
{
    [Fact]
    public async Task Production_RequiresAuthenticationAndKeepsHealthAnonymous()
    {
        var variables = new Dictionary<string, string?>
        {
            ["ApiSecurity__RequireAuthentication"] = "true",
            ["ApiSecurity__Authority"] = "https://identity.example.com",
            ["ApiSecurity__Audience"] = "smart-mill-sync",
            ["Cors__AllowedOrigins__0"] = "https://mill.example.com",
            ["ConnectionStrings__SmartMillDb"] = "Host=localhost;Port=5432;Database=smart_mill_sync;Username=postgres;Password=postgres",
            ["AllowedHosts"] = "localhost"
        };
        var previousValues = variables.Keys.ToDictionary(
            key => key,
            Environment.GetEnvironmentVariable);

        try
        {
            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
            }

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var deliveries = await client.GetAsync("/api/v1/deliveries");
            var hub = await client.PostAsync("/hubs/mill-sync/negotiate?negotiateVersion=1", null);
            var liveness = await client.GetAsync("/health/live");
            var swagger = await client.GetAsync("/swagger/v1/swagger.json");

            Assert.Equal(HttpStatusCode.Unauthorized, deliveries.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, hub.StatusCode);
            Assert.Equal(HttpStatusCode.OK, liveness.StatusCode);
            Assert.True(
                swagger.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized,
                $"Swagger must not be publicly available in Production; actual status was {swagger.StatusCode}.");
        }
        finally
        {
            foreach (var variable in previousValues)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
            }
        }
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public sealed class EnvironmentVariablesCollection;

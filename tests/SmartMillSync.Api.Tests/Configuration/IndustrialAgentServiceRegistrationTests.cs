using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartMillSync.Api.Configuration;
using SmartMillSync.Api.Services;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Plugins;
using Xunit;

namespace SmartMillSync.Api.Tests.Configuration;

public sealed class IndustrialAgentServiceRegistrationTests
{
    [Fact]
    public void AddIndustrialAgent_WithoutApiKey_RegistersOperationalFallback()
    {
        var previousApiKey = Environment.GetEnvironmentVariable("GOOGLE_AI_API_KEY");
        Environment.SetEnvironmentVariable("GOOGLE_AI_API_KEY", null);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["IndustrialAgent:ModelId"] = "gemini-2.0-flash",
                    ["IndustrialAgent:GasPricePerNm3"] = "2.50"
                })
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddIndustrialAgent(configuration);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            Assert.IsType<UnavailableIndustrialAgentChatGateway>(
                scope.ServiceProvider.GetRequiredService<IIndustrialAgentChatGateway>());
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IIndustrialAgentService));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(MillTelemetryPlugin));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_AI_API_KEY", previousApiKey);
        }
    }
}

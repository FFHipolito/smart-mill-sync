using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SmartMillSync.Api.Configuration;
using Xunit;

namespace SmartMillSync.Api.Tests.Configuration;

public sealed class ApiSecurityRegistrationTests
{
    [Fact]
    public void AddApiSecurity_WhenDisabled_DoesNotRegisterAuthentication()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(false, null, null);

        var enabled = services.AddApiSecurity(configuration, Environment("Development"));

        Assert.False(enabled);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAuthenticationService));
    }

    [Fact]
    public void AddApiSecurity_WhenEnabledWithoutOidcConfiguration_FailsFast()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(true, null, null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddApiSecurity(configuration, Environment("Production")));

        Assert.Contains("Authority", exception.Message);
        Assert.Contains("Audience", exception.Message);
    }

    [Fact]
    public void AddApiSecurity_WhenConfigured_RegistersJwtAuthentication()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(
            true,
            "https://identity.example.com",
            "smart-mill-sync");

        var enabled = services.AddApiSecurity(configuration, Environment("Production"));

        Assert.True(enabled);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAuthenticationService));
    }

    [Fact]
    public void AddApiSecurity_WhenDisabledOutsideDevelopment_FailsClosed()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(false, null, null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddApiSecurity(configuration, Environment("Production")));

        Assert.Contains("only be disabled in Development", exception.Message);
    }

    private static IHostEnvironment Environment(string environmentName) => new TestHostEnvironment
    {
        EnvironmentName = environmentName
    };

    private static IConfiguration BuildConfiguration(
        bool enabled,
        string? authority,
        string? audience) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ApiSecurity:RequireAuthentication"] = enabled.ToString(),
            ["ApiSecurity:Authority"] = authority,
            ["ApiSecurity:Audience"] = audience
        })
        .Build();

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "SmartMillSync.Api.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

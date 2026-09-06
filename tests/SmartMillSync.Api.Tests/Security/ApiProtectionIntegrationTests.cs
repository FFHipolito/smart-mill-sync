using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmartMillSync.Api.Tests.Security;

public sealed class ApiProtectionIntegrationTests
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_AreAvailable(string path)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_AllowsConfiguredClientAndRejectsUnknownOrigin()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var allowed = new HttpRequestMessage(HttpMethod.Options, "/api/v1/deliveries");
        allowed.Headers.Add("Origin", "http://localhost:5080");
        allowed.Headers.Add("Access-Control-Request-Method", "GET");
        using var allowedResponse = await client.SendAsync(allowed);
        using var denied = new HttpRequestMessage(HttpMethod.Options, "/api/v1/deliveries");
        denied.Headers.Add("Origin", "https://attacker.example");
        denied.Headers.Add("Access-Control-Request-Method", "GET");
        using var deniedResponse = await client.SendAsync(denied);

        Assert.Equal("http://localhost:5080", allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(deniedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task OversizedAgentBody_IsRejected()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = new
        {
            message = new string('x', 70_000),
            history = Array.Empty<object>()
        };

        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task InvalidAgentBody_ReturnsValidationProblemDetails()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/agent/chat", new
        {
            message = "",
            history = Array.Empty<object>()
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", body);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AgentRateLimit_ReturnsProblemDetailsAfterPermitBudget()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        HttpResponseMessage? limitedResponse = null;

        for (var index = 0; index < 7; index++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/agent/chat", new
            {
                message = "",
                history = Array.Empty<object>()
            });
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                limitedResponse = response;
                break;
            }
            response.Dispose();
        }

        Assert.NotNull(limitedResponse);
        var body = await limitedResponse.Content.ReadAsStringAsync();
        Assert.Contains("Request rate limit exceeded", body);
        limitedResponse.Dispose();
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;

namespace SmartMillSync.Api.Tests.OpenApi;

public sealed class OpenApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenApiContractTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Swagger_DescribesEveryEndpointWithOperationsAndErrorResponses()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        AssertOperation(paths, "/api/v1/deliveries", "post", "RegisterWoodDelivery", "201", "400", "413", "429", "500");
        AssertOperation(paths, "/api/v1/deliveries", "get", "GetActiveWoodDeliveries", "200", "429", "500");
        AssertOperation(paths, "/api/v1/energy-balance", "get", "GetMillEnergyBalance", "200", "429", "500");
        AssertOperation(paths, "/api/v1/agent/chat", "post", "ChatWithIndustrialAgent", "200", "400", "413", "429", "500", "503");
        AssertOperation(paths, "/api/v1/agent/diagnose-delivery/{id}", "post", "DiagnoseWoodDelivery", "200", "404", "429", "500", "503");
    }

    [Fact]
    public async Task Swagger_ExposesBodyConstraintsAndExamples()
    {
        using var client = _factory.CreateClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        var delivery = schemas.GetProperty("CreateWoodDeliveryRequest").GetProperty("properties");
        var chat = schemas.GetProperty("IndustrialAgentChatRequest").GetProperty("properties");

        Assert.Equal(7, delivery.GetProperty("truckPlate").GetProperty("maxLength").GetInt32());
        Assert.Equal("ABC1D23", delivery.GetProperty("truckPlate").GetProperty("example").GetString());
        Assert.Equal(70m, delivery.GetProperty("moisturePercentage").GetProperty("maximum").GetDecimal());
        Assert.Equal(2_000, chat.GetProperty("message").GetProperty("maxLength").GetInt32());
        Assert.Equal(20, chat.GetProperty("history").GetProperty("maxItems").GetInt32());
    }

    private static void AssertOperation(
        JsonElement paths,
        string path,
        string method,
        string operationId,
        params string[] expectedResponses)
    {
        var operation = paths.GetProperty(path).GetProperty(method);
        Assert.Equal(operationId, operation.GetProperty("operationId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(operation.GetProperty("summary").GetString()));
        var responses = operation.GetProperty("responses");
        Assert.All(expectedResponses, status =>
        {
            Assert.True(responses.TryGetProperty(status, out var response), $"Missing {status} on {operationId}.");
            if (int.Parse(status) >= 400)
            {
                Assert.True(
                    response.GetProperty("content").TryGetProperty("application/problem+json", out _),
                    $"Error {status} on {operationId} must use application/problem+json.");
            }
        });
    }
}

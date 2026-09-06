using System.Net;
using System.Text;
using System.Text.Json;
using SmartMillSync.Client.Services;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Client.Tests.Services;

public sealed class IndustrialAgentApiClientTests
{
    [Fact]
    public async Task ChatAsync_PostsHistoryAndReturnsAnalysis()
    {
        var expected = new IndustrialAgentResponse(
            "Secagem recomendada.", DateTimeOffset.UtcNow, "gemini-2.0-flash");
        var handler = new StubHttpHandler(JsonSerializer.Serialize(expected));
        var client = CreateClient(handler);
        var request = new IndustrialAgentChatRequest("Avalie", []);

        var result = await client.ChatAsync(request);

        Assert.Equal(expected, result);
        Assert.Equal("/api/v1/agent/chat", handler.LastRequest!.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task DiagnoseDeliveryAsync_PostsDeliveryRoute()
    {
        var deliveryId = Guid.NewGuid();
        var expected = new IndustrialAgentResponse(
            "Alerta alto.", DateTimeOffset.UtcNow, "gemini-2.0-flash", deliveryId);
        var handler = new StubHttpHandler(JsonSerializer.Serialize(expected));
        var client = CreateClient(handler);

        var result = await client.DiagnoseDeliveryAsync(deliveryId);

        Assert.Equal(deliveryId, result.DeliveryId);
        Assert.Equal($"/api/v1/agent/diagnose-delivery/{deliveryId}", handler.LastRequest!.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ChatAsync_WhenAgentIsUnavailable_ThrowsTypedException()
    {
        var handler = new StubHttpHandler("{\"title\":\"Industrial agent unavailable\"}", HttpStatusCode.ServiceUnavailable);
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<IndustrialAgentApiException>(() =>
            client.ChatAsync(new IndustrialAgentChatRequest("Avalie", [])));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
    }

    private static IndustrialAgentApiClient CreateClient(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5243/") });

    private sealed class StubHttpHandler(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}

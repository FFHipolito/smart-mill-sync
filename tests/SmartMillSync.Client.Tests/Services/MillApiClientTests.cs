using System.Net;
using System.Text;
using System.Text.Json;
using SmartMillSync.Client.Services;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Client.Tests.Services;

public sealed class MillApiClientTests
{
    [Fact]
    public async Task GetActiveDeliveriesAsync_DeserializesApiResponse()
    {
        var expected = CreateResponse();
        var client = CreateClient(JsonSerializer.Serialize(new[] { expected }));

        var result = await client.GetActiveDeliveriesAsync();

        Assert.Equal(expected, Assert.Single(result));
    }

    [Fact]
    public async Task RegisterDeliveryAsync_PostsRequestAndReturnsCreatedDelivery()
    {
        var expected = CreateResponse();
        var handler = new StubHttpHandler(JsonSerializer.Serialize(expected));
        var client = new MillApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5243/")
        });
        var request = new CreateWoodDeliveryRequest("ABC1D23", "Origin", "Species", 50m, 10m, 45m);

        var result = await client.RegisterDeliveryAsync(request);

        Assert.Equal(expected, result);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/v1/deliveries", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    private static MillApiClient CreateClient(string json) => new(
        new HttpClient(new StubHttpHandler(json)) { BaseAddress = new Uri("http://localhost:5243/") });

    private static WoodDeliveryResponse CreateResponse() => new(
        Guid.NewGuid(), "ABC1D23", "Origin", "Species", 50m, 10m, 40m, 45m,
        22m, DeliveryStatus.ArrivedAtGate, false, 0m, AlertLevel.Normal,
        DateTimeOffset.UtcNow);

    private sealed class StubHttpHandler(string responseJson) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}

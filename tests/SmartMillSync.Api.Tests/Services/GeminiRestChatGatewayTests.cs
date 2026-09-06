using System.Net;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartMillSync.Api.Services;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Application.Plugins;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Api.Tests.Services;

public sealed class GeminiRestChatGatewayTests
{
    [Fact]
    public async Task CompleteAsync_PreservesFunctionMetadataAndReturnsGroundedAnalysis()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetMillEnergyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(new EnergyBalanceSummaryDto(1, 40m, 18m, 770m, 55m, AlertLevel.High));
        var plugin = new MillTelemetryPlugin(
            sender,
            Options.Create(new IndustrialAgentOptions { GasPricePerNm3 = 2.50m }));
        var handler = new SequencedHandler(
            """
            {
              "candidates": [{
                "content": {
                  "role": "model",
                  "parts": [{
                    "functionCall": {
                      "name": "get_current_energy_balance",
                      "args": {},
                      "id": "call-123"
                    },
                    "thoughtSignature": "signature-abc"
                  }]
                }
              }]
            }
            """,
            """
            {
              "candidates": [{
                "content": {
                  "role": "model",
                  "parts": [{ "text": "Alerta alto: enviar a carga para secagem natural." }]
                }
              }]
            }
            """);
        var gateway = new GeminiRestChatGateway(
            new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") },
            plugin,
            Options.Create(new IndustrialAgentOptions { ModelId = "gemini-3.1-flash-lite" }));

        var result = await gateway.CompleteAsync(
            "Use telemetria.",
            [],
            "Analise o patio.",
            CancellationToken.None);

        Assert.Contains("secagem natural", result);
        Assert.Equal(2, handler.RequestBodies.Count);
        var continuation = handler.RequestBodies[1];
        Assert.Contains("signature-abc", continuation);
        Assert.Contains("call-123", continuation);
        Assert.Contains("estimatedAdditionalGasNm3", continuation);
        Assert.Contains("770", continuation);
    }

    [Fact]
    public async Task CompleteAsync_WhenGeminiRejectsRequest_ThrowsTypedException()
    {
        var sender = Substitute.For<ISender>();
        var plugin = new MillTelemetryPlugin(
            sender,
            Options.Create(new IndustrialAgentOptions()));
        var handler = new SequencedHandler(
            "{\"error\":{\"message\":\"Model unavailable\"}}",
            HttpStatusCode.ServiceUnavailable);
        var gateway = new GeminiRestChatGateway(
            new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") },
            plugin,
            Options.Create(new IndustrialAgentOptions()));

        var exception = await Assert.ThrowsAsync<GeminiRequestException>(() =>
            gateway.CompleteAsync("system", [], "message", CancellationToken.None));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
        Assert.Equal("Model unavailable", exception.Message);
    }

    private sealed class SequencedHandler : HttpMessageHandler
    {
        private readonly Queue<(string Body, HttpStatusCode StatusCode)> _responses;

        public SequencedHandler(params string[] responses)
            : this(responses.Select(response => (response, HttpStatusCode.OK)).ToArray())
        {
        }

        public SequencedHandler(string response, HttpStatusCode statusCode)
            : this([(response, statusCode)])
        {
        }

        private SequencedHandler(params (string Body, HttpStatusCode StatusCode)[] responses) =>
            _responses = new Queue<(string Body, HttpStatusCode StatusCode)>(responses);

        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            var response = _responses.Dequeue();
            return new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
            };
        }
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Plugins;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Services;

public sealed class GeminiRestChatGateway(
    HttpClient httpClient,
    MillTelemetryPlugin telemetryPlugin,
    IOptions<IndustrialAgentOptions> options)
    : IIndustrialAgentChatGateway
{
    private const int MaximumToolCalls = 6;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static readonly object[] ToolDeclarations =
    [
        new
        {
            functionDeclarations = new object[]
            {
                new
                {
                    name = "get_current_energy_balance",
                    description = "Returns today's consolidated wood-yard energy balance using live operational data.",
                    parameters = new { type = "OBJECT", properties = new { } }
                },
                new
                {
                    name = "analyze_moisture_impact",
                    description = "Calculates extra natural gas volume and estimated cost from moisture and net tonnage.",
                    parameters = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            moisturePercentage = new { type = "NUMBER", description = "Wood moisture percentage from 10 to 70." },
                            tonnage = new { type = "NUMBER", description = "Net wood tonnage greater than zero." }
                        },
                        required = new[] { "moisturePercentage", "tonnage" }
                    }
                },
                new
                {
                    name = "get_high_moisture_deliveries",
                    description = "Returns active wood deliveries with moisture above 50 percent.",
                    parameters = new { type = "OBJECT", properties = new { } }
                }
            }
        }
    ];
    private readonly IndustrialAgentOptions _options = options.Value;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<AgentChatMessageDto> history,
        string message,
        CancellationToken cancellationToken)
    {
        var contents = history.Select(MapHistoryMessage).ToList();
        contents.Add(new GeminiContent("user", [new GeminiPart(Text: message)]));

        for (var callCount = 0; callCount <= MaximumToolCalls; callCount++)
        {
            var request = new GeminiRequest(
                new GeminiSystemInstruction([new GeminiPart(Text: systemPrompt)]),
                contents,
                ToolDeclarations);
            using var response = await httpClient.PostAsJsonAsync(
                $"v1/models/{_options.ModelId}:generateContent",
                request,
                JsonOptions,
                cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var payload = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Gemini returned an empty response.");
            var modelContent = payload.Candidates?.FirstOrDefault()?.Content
                ?? throw new InvalidOperationException("Gemini returned no candidate content.");
            var functionCalls = modelContent.Parts
                .Where(part => part.FunctionCall is not null)
                .ToArray();

            if (functionCalls.Length == 0)
            {
                var text = string.Join("\n", modelContent.Parts
                    .Select(part => part.Text)
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
                return string.IsNullOrWhiteSpace(text)
                    ? "Nao foi possivel gerar uma analise industrial conclusiva."
                    : text.Trim();
            }

            contents.Add(modelContent);
            var functionResponses = new List<GeminiPart>(functionCalls.Length);
            foreach (var callPart in functionCalls)
            {
                var call = callPart.FunctionCall!;
                var result = await InvokeToolAsync(call, cancellationToken);
                functionResponses.Add(new GeminiPart(FunctionResponse: new GeminiFunctionResponse(
                    call.Name,
                    call.Id,
                    new Dictionary<string, object?> { ["result"] = result })));
            }

            contents.Add(new GeminiContent("user", functionResponses));
        }

        throw new InvalidOperationException("Gemini exceeded the maximum number of telemetry tool calls.");
    }

    private async Task<object?> InvokeToolAsync(
        GeminiFunctionCall call,
        CancellationToken cancellationToken) => call.Name switch
    {
        "get_current_energy_balance" =>
            await telemetryPlugin.GetCurrentEnergyBalanceAsync(cancellationToken),
        "get_high_moisture_deliveries" =>
            await telemetryPlugin.GetHighMoistureDeliveriesAsync(cancellationToken),
        "analyze_moisture_impact" => telemetryPlugin.AnalyzeMoistureImpact(
            GetRequiredDecimal(call.Args, "moisturePercentage"),
            GetRequiredDecimal(call.Args, "tonnage")),
        _ => throw new InvalidOperationException($"Gemini requested unknown tool '{call.Name}'.")
    };

    private static decimal GetRequiredDecimal(JsonElement arguments, string propertyName) =>
        arguments.TryGetProperty(propertyName, out var value) && value.TryGetDecimal(out var number)
            ? number
            : throw new ArgumentException($"Tool argument '{propertyName}' is required.");

    private static GeminiContent MapHistoryMessage(AgentChatMessageDto message) => new(
        message.Role == AgentMessageRole.User ? "user" : "model",
        [new GeminiPart(Text: message.Content)]);

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<GeminiErrorEnvelope>(JsonOptions, cancellationToken);
        throw new GeminiRequestException(
            error?.Error?.Message ?? "Gemini request failed.",
            response.StatusCode);
    }

    private sealed record GeminiRequest(
        GeminiSystemInstruction SystemInstruction,
        IReadOnlyList<GeminiContent> Contents,
        IReadOnlyList<object> Tools);

    private sealed record GeminiSystemInstruction(IReadOnlyList<GeminiPart> Parts);

    private sealed record GeminiContent(string Role, IReadOnlyList<GeminiPart> Parts);

    private sealed record GeminiPart(
        string? Text = null,
        GeminiFunctionCall? FunctionCall = null,
        GeminiFunctionResponse? FunctionResponse = null,
        string? ThoughtSignature = null);

    private sealed record GeminiFunctionCall(string Name, JsonElement Args, string? Id);

    private sealed record GeminiFunctionResponse(
        string Name,
        string? Id,
        IReadOnlyDictionary<string, object?> Response);

    private sealed record GeminiResponse(IReadOnlyList<GeminiCandidate>? Candidates);

    private sealed record GeminiCandidate(GeminiContent Content);

    private sealed record GeminiErrorEnvelope(GeminiError? Error);

    private sealed record GeminiError(string? Message);
}

public sealed class GeminiRequestException(
    string message,
    System.Net.HttpStatusCode statusCode)
    : Exception(message)
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;
}

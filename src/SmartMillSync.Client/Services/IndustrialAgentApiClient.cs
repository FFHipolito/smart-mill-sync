using System.Net.Http.Json;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Client.Services;

public interface IIndustrialAgentApiClient
{
    Task<IndustrialAgentResponse> ChatAsync(
        IndustrialAgentChatRequest request,
        CancellationToken cancellationToken = default);

    Task<IndustrialAgentResponse> DiagnoseDeliveryAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default);
}

public sealed class IndustrialAgentApiClient(HttpClient httpClient) : IIndustrialAgentApiClient
{
    public async Task<IndustrialAgentResponse> ChatAsync(
        IndustrialAgentChatRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/agent/chat",
            request,
            cancellationToken);
        await EnsureAgentSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<IndustrialAgentResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The agent returned an empty response.");
    }

    public async Task<IndustrialAgentResponse> DiagnoseDeliveryAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"api/v1/agent/diagnose-delivery/{deliveryId}",
            null,
            cancellationToken);
        await EnsureAgentSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<IndustrialAgentResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The agent returned an empty diagnosis.");
    }

    private static async Task EnsureAgentSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<AgentProblemDetails>(cancellationToken);
        throw new IndustrialAgentApiException(
            problem?.Title ?? "Industrial agent request failed.",
            response.StatusCode);
    }

    private sealed record AgentProblemDetails(string? Title);
}

public sealed class IndustrialAgentApiException(
    string message,
    System.Net.HttpStatusCode statusCode)
    : Exception(message)
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;
}

using System.Net.Http.Json;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Client.Services;

public sealed class MillApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<WoodDeliveryResponse>> GetActiveDeliveriesAsync(
        CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<WoodDeliveryResponse[]>(
            "api/v1/deliveries",
            cancellationToken) ?? [];

    public async Task<EnergyBalanceSummaryDto> GetEnergyBalanceAsync(
        CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<EnergyBalanceSummaryDto>(
            "api/v1/energy-balance",
            cancellationToken)
        ?? throw new InvalidOperationException("The API returned an empty energy balance response.");

    public async Task<WoodDeliveryResponse> RegisterDeliveryAsync(
        CreateWoodDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/deliveries",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WoodDeliveryResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty delivery response.");
    }
}

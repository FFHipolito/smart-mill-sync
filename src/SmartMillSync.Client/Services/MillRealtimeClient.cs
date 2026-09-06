using Microsoft.AspNetCore.SignalR.Client;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Client.Services;

public sealed class MillRealtimeClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public MillRealtimeClient(HttpClient httpClient)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress!, "hubs/mill-sync"))
            .WithAutomaticReconnect()
            .Build();
    }

    public event Func<WoodDeliveryResponse, Task>? DeliveryUpdated;
    public event Func<EnergyBalanceSummaryDto, Task>? EnergyBalanceUpdated;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _connection.On<WoodDeliveryResponse>(
            "DeliveryUpdated",
            delivery => DeliveryUpdated?.Invoke(delivery) ?? Task.CompletedTask);
        _connection.On<EnergyBalanceSummaryDto>(
            "EnergyBalanceUpdated",
            summary => EnergyBalanceUpdated?.Invoke(summary) ?? Task.CompletedTask);

        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

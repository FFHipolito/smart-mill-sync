using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Queries;

namespace SmartMillSync.Infrastructure.Workers;

public sealed class ThermalBalanceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ThermalBalanceWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await RunCycleAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var notificationService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
            var summary = await sender.Send(new GetMillEnergyBalanceQuery(), cancellationToken);

            if (summary.EstimatedAdditionalGasNm3 > 0m)
            {
                await notificationService.NotifyEnergyBalanceAsync(summary, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish the thermal balance update.");
        }
    }
}

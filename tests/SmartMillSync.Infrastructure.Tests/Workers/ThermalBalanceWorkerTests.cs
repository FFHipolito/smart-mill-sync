using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Infrastructure.Workers;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Workers;

public sealed class ThermalBalanceWorkerTests
{
    [Fact]
    public async Task RunCycleAsync_WithAdditionalGas_NotifiesClients()
    {
        var sender = Substitute.For<ISender>();
        var notifications = Substitute.For<ISignalRNotificationService>();
        var summary = new EnergyBalanceSummaryDto(1, 40m, 18m, 770m, 55m, AlertLevel.High);
        sender.Send(Arg.Any<GetMillEnergyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(summary);
        var worker = CreateWorker(sender, notifications);

        await worker.RunCycleAsync(CancellationToken.None);

        await notifications.Received(1).NotifyEnergyBalanceAsync(summary, CancellationToken.None);
    }

    [Fact]
    public async Task RunCycleAsync_WithoutAdditionalGas_DoesNotNotifyClients()
    {
        var sender = Substitute.For<ISender>();
        var notifications = Substitute.For<ISignalRNotificationService>();
        sender.Send(Arg.Any<GetMillEnergyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(new EnergyBalanceSummaryDto(1, 40m, 22m, 0m, 45m, AlertLevel.Normal));
        var worker = CreateWorker(sender, notifications);

        await worker.RunCycleAsync(CancellationToken.None);

        await notifications.DidNotReceiveWithAnyArgs().NotifyEnergyBalanceAsync(default!, default);
    }

    private static ThermalBalanceWorker CreateWorker(
        ISender sender,
        ISignalRNotificationService notifications)
    {
        var services = new ServiceCollection();
        services.AddSingleton(sender);
        services.AddSingleton(notifications);
        return new ThermalBalanceWorker(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ThermalBalanceWorker>.Instance);
    }
}

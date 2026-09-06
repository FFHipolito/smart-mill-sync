using System.ComponentModel;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Domain.Entities;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Plugins;

public sealed record MoistureImpactAnalysis(
    decimal MoisturePercentage,
    decimal Tonnage,
    decimal ExtraGasVolumeNm3,
    decimal EstimatedCost);

public sealed class MillTelemetryPlugin(
    ISender sender,
    IOptions<IndustrialAgentOptions> options)
{
    private readonly IndustrialAgentOptions _options = options.Value;

    [KernelFunction("get_current_energy_balance")]
    [Description("Returns today's consolidated wood-yard energy balance using live operational data.")]
    public Task<EnergyBalanceSummaryDto> GetCurrentEnergyBalanceAsync(
        CancellationToken cancellationToken = default) =>
        sender.Send(new GetMillEnergyBalanceQuery(), cancellationToken);

    [KernelFunction("analyze_moisture_impact")]
    [Description("Calculates extra natural gas volume and estimated cost from moisture and net tonnage.")]
    public MoistureImpactAnalysis AnalyzeMoistureImpact(
        [Description("Wood moisture percentage between 10 and 70.")] decimal moisturePercentage,
        [Description("Net wood tonnage greater than zero.")] decimal tonnage)
    {
        if (moisturePercentage is < 10m or > 70m)
        {
            throw new ArgumentOutOfRangeException(nameof(moisturePercentage));
        }

        if (tonnage <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(tonnage));
        }

        var extraGasVolume = moisturePercentage > WoodDelivery.ThermalCompensationThreshold
            ? (moisturePercentage - WoodDelivery.ThermalCompensationThreshold)
                * WoodDelivery.GasVolumePerMoisturePointPerTon
                * tonnage
            : 0m;

        return new MoistureImpactAnalysis(
            moisturePercentage,
            tonnage,
            extraGasVolume,
            extraGasVolume * _options.GasPricePerNm3);
    }

    [KernelFunction("get_high_moisture_deliveries")]
    [Description("Returns active wood deliveries with moisture above 50 percent.")]
    public async Task<IReadOnlyList<WoodDeliveryResponse>> GetHighMoistureDeliveriesAsync(
        CancellationToken cancellationToken = default)
    {
        var deliveries = await sender.Send(new GetActiveDeliveriesQuery(), cancellationToken);
        return deliveries
            .Where(delivery => delivery.MoisturePercentage > WoodDelivery.ThermalCompensationThreshold)
            .OrderByDescending(delivery => delivery.MoisturePercentage)
            .ToArray();
    }
}

namespace SmartMillSync.Shared.DTOs;

public sealed record CreateWoodDeliveryRequest(
    string TruckPlate,
    string ForestOrigin,
    string WoodSpecies,
    decimal GrossWeight,
    decimal TareWeight,
    decimal MoisturePercentage);

public sealed record WoodDeliveryResponse(
    Guid Id,
    string TruckPlate,
    string ForestOrigin,
    string WoodSpecies,
    decimal GrossWeight,
    decimal TareWeight,
    decimal NetWeight,
    decimal MoisturePercentage,
    decimal DryWeightTons,
    DeliveryStatus Status,
    bool RequiresThermalCompensation,
    decimal ExtraGasVolume,
    AlertLevel AlertLevel,
    DateTimeOffset ArrivedAtUtc);

public sealed record EnergyBalanceSummaryDto(
    int TotalDeliveries,
    decimal TotalNetWeightTons,
    decimal AccumulatedDryBiomassTons,
    decimal EstimatedAdditionalGasNm3,
    decimal AverageMoisturePercentage,
    AlertLevel AlertLevel);

public enum DeliveryStatus
{
    InTransit = 1,
    ArrivedAtGate = 2,
    Weighed = 3,
    Unloading = 4,
    Completed = 5,
    Rejected = 6
}

public enum AlertLevel
{
    Normal = 1,
    Moderate = 2,
    High = 3
}

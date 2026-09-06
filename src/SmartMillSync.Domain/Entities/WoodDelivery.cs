using System.Text.RegularExpressions;
using SmartMillSync.Domain.Enums;

namespace SmartMillSync.Domain.Entities;

public sealed partial class WoodDelivery
{
    public const decimal ThermalCompensationThreshold = 50m;
    public const decimal GasVolumePerMoisturePointPerTon = 3.85m;
    public const decimal HighAlertThreshold = 150m;

    private WoodDelivery()
    {
        TruckPlate = string.Empty;
        ForestOrigin = string.Empty;
        WoodSpecies = string.Empty;
    }

    public WoodDelivery(
        string truckPlate,
        string forestOrigin,
        string woodSpecies,
        decimal grossWeight,
        decimal tareWeight,
        decimal moisturePercentage)
    {
        var normalizedPlate = NormalizePlate(truckPlate);

        if (!TruckPlatePattern().IsMatch(normalizedPlate))
        {
            throw new ArgumentException("Truck plate must use a valid Brazilian format.", nameof(truckPlate));
        }

        TruckPlate = normalizedPlate;
        ForestOrigin = RequireText(forestOrigin, nameof(forestOrigin));
        WoodSpecies = RequireText(woodSpecies, nameof(woodSpecies));

        if (grossWeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossWeight), "Gross weight must be positive.");
        }

        if (tareWeight < 0 || tareWeight >= grossWeight)
        {
            throw new ArgumentOutOfRangeException(nameof(tareWeight), "Tare weight must be non-negative and lower than gross weight.");
        }

        if (moisturePercentage is < 10m or > 70m)
        {
            throw new ArgumentOutOfRangeException(nameof(moisturePercentage), "Moisture must be between 10% and 70%.");
        }

        Id = Guid.NewGuid();
        GrossWeight = grossWeight;
        TareWeight = tareWeight;
        MoisturePercentage = moisturePercentage;
        Status = DeliveryStatus.ArrivedAtGate;
        ArrivedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string TruckPlate { get; private set; }
    public string ForestOrigin { get; private set; }
    public string WoodSpecies { get; private set; }
    public decimal GrossWeight { get; private set; }
    public decimal TareWeight { get; private set; }
    public decimal NetWeight => GrossWeight - TareWeight;
    public decimal MoisturePercentage { get; private set; }
    public decimal DryWeightTons => NetWeight * (1m - MoisturePercentage / 100m);
    public DeliveryStatus Status { get; private set; }
    public DateTimeOffset ArrivedAtUtc { get; private set; }
    public bool RequiresThermalCompensation => MoisturePercentage > ThermalCompensationThreshold;
    public decimal ExtraGasVolume => RequiresThermalCompensation
        ? (MoisturePercentage - ThermalCompensationThreshold) * GasVolumePerMoisturePointPerTon * NetWeight
        : 0m;
    public bool HasHighGasAlert => ExtraGasVolume > HighAlertThreshold;

    public void ChangeStatus(DeliveryStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        Status = status;
    }

    private static string NormalizePlate(string truckPlate) =>
        string.Concat((truckPlate ?? string.Empty).Where(char.IsLetterOrDigit)).ToUpperInvariant();

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value.Trim();

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex TruckPlatePattern();
}

namespace SmartMillSync.Domain.Entities;

public sealed class GasTelemetryRecord
{
    private GasTelemetryRecord()
    {
    }

    public GasTelemetryRecord(
        decimal flowRateNm3PerHour,
        decimal temperatureCelsius,
        decimal thermalDeviationPercentage,
        DateTimeOffset? recordedAtUtc = null)
    {
        if (flowRateNm3PerHour < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(flowRateNm3PerHour));
        }

        if (temperatureCelsius is < -273.15m or > 2_000m)
        {
            throw new ArgumentOutOfRangeException(nameof(temperatureCelsius));
        }

        Id = Guid.NewGuid();
        FlowRateNm3PerHour = flowRateNm3PerHour;
        TemperatureCelsius = temperatureCelsius;
        ThermalDeviationPercentage = thermalDeviationPercentage;
        RecordedAtUtc = recordedAtUtc ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public decimal FlowRateNm3PerHour { get; private set; }
    public decimal TemperatureCelsius { get; private set; }
    public decimal ThermalDeviationPercentage { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
}

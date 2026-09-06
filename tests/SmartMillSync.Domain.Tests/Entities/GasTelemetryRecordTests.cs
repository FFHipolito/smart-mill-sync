using SmartMillSync.Domain.Entities;
using Xunit;

namespace SmartMillSync.Domain.Tests.Entities;

public sealed class GasTelemetryRecordTests
{
    [Fact]
    public void Constructor_WithValidReadings_CreatesRecord()
    {
        var recordedAt = DateTimeOffset.Parse("2026-09-06T12:00:00Z");

        var record = new GasTelemetryRecord(1_250.50m, 182.4m, -2.7m, recordedAt);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(1_250.50m, record.FlowRateNm3PerHour);
        Assert.Equal(182.4m, record.TemperatureCelsius);
        Assert.Equal(-2.7m, record.ThermalDeviationPercentage);
        Assert.Equal(recordedAt, record.RecordedAtUtc);
    }

    [Theory]
    [InlineData(-0.01, 100)]
    [InlineData(10, -273.16)]
    [InlineData(10, 2000.01)]
    public void Constructor_WithImpossibleReading_Throws(decimal flowRate, decimal temperature)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GasTelemetryRecord(flowRate, temperature, 0m));
    }

    [Fact]
    public void PublicProperties_DoNotExposeSetters()
    {
        var writableProperties = typeof(GasTelemetryRecord)
            .GetProperties()
            .Where(property => property.SetMethod?.IsPublic == true);

        Assert.Empty(writableProperties);
    }
}

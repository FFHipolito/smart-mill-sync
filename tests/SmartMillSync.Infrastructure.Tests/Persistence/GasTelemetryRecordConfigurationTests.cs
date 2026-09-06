using Microsoft.EntityFrameworkCore;
using SmartMillSync.Domain.Entities;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Persistence;

public sealed class GasTelemetryRecordConfigurationTests
{
    [Fact]
    public void Model_ConfiguresTelemetryTableAndDecimalPrecision()
    {
        using var context = SmartMillDbContextTests.CreateContext();
        var entity = context.Model.FindEntityType(typeof(GasTelemetryRecord));

        Assert.NotNull(entity);
        Assert.Equal("gas_telemetry_records", entity.GetTableName());
        Assert.Equal(nameof(GasTelemetryRecord.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.All(
            new[]
            {
                nameof(GasTelemetryRecord.FlowRateNm3PerHour),
                nameof(GasTelemetryRecord.TemperatureCelsius),
                nameof(GasTelemetryRecord.ThermalDeviationPercentage)
            },
            propertyName =>
            {
                var property = entity.FindProperty(propertyName)!;
                Assert.Equal(18, property.GetPrecision());
                Assert.Equal(2, property.GetScale());
            });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMillSync.Domain.Entities;

namespace SmartMillSync.Infrastructure.Persistence.Configurations;

public sealed class GasTelemetryRecordConfiguration : IEntityTypeConfiguration<GasTelemetryRecord>
{
    public void Configure(EntityTypeBuilder<GasTelemetryRecord> builder)
    {
        builder.ToTable("gas_telemetry_records");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.FlowRateNm3PerHour).HasPrecision(18, 2);
        builder.Property(record => record.TemperatureCelsius).HasPrecision(18, 2);
        builder.Property(record => record.ThermalDeviationPercentage).HasPrecision(18, 2);
        builder.Property(record => record.RecordedAtUtc).IsRequired();
        builder.HasIndex(record => record.RecordedAtUtc);
    }
}

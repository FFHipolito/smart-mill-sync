using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMillSync.Domain.Entities;

namespace SmartMillSync.Infrastructure.Persistence.Configurations;

public sealed class WoodDeliveryConfiguration : IEntityTypeConfiguration<WoodDelivery>
{
    public void Configure(EntityTypeBuilder<WoodDelivery> builder)
    {
        builder.ToTable("wood_deliveries");
        builder.HasKey(delivery => delivery.Id);
        builder.Property(delivery => delivery.TruckPlate).HasMaxLength(7).IsRequired();
        builder.Property(delivery => delivery.ForestOrigin).HasMaxLength(160).IsRequired();
        builder.Property(delivery => delivery.WoodSpecies).HasMaxLength(120).IsRequired();
        builder.Property(delivery => delivery.GrossWeight).HasPrecision(18, 2);
        builder.Property(delivery => delivery.TareWeight).HasPrecision(18, 2);
        builder.Property(delivery => delivery.MoisturePercentage).HasPrecision(18, 2);
        builder.Property(delivery => delivery.Status).HasConversion<int>();
        builder.Property(delivery => delivery.ArrivedAtUtc).IsRequired();
        builder.Ignore(delivery => delivery.NetWeight);
        builder.Ignore(delivery => delivery.DryWeightTons);
        builder.Ignore(delivery => delivery.RequiresThermalCompensation);
        builder.Ignore(delivery => delivery.ExtraGasVolume);
        builder.Ignore(delivery => delivery.HasHighGasAlert);
        builder.HasIndex(delivery => delivery.ArrivedAtUtc);
        builder.HasIndex(delivery => delivery.Status);
    }
}

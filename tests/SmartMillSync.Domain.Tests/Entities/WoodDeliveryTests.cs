using SmartMillSync.Domain.Entities;
using SmartMillSync.Domain.Enums;
using Xunit;

namespace SmartMillSync.Domain.Tests.Entities;

public sealed class WoodDeliveryTests
{
    [Fact]
    public void Constructor_NormalizesPlateAndCalculatesEnergyMetrics()
    {
        var delivery = new WoodDelivery("abc-1d23", "Mucuri-04", "Eucalyptus", 50m, 10m, 55m);

        Assert.Equal("ABC1D23", delivery.TruckPlate);
        Assert.Equal(40m, delivery.NetWeight);
        Assert.Equal(18m, delivery.DryWeightTons);
        Assert.Equal(770m, delivery.ExtraGasVolume);
        Assert.True(delivery.RequiresThermalCompensation);
        Assert.True(delivery.HasHighGasAlert);
        Assert.Equal(DeliveryStatus.ArrivedAtGate, delivery.Status);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(45)]
    public void ExtraGasVolume_AtOrBelowThreshold_IsZero(decimal moisture)
    {
        var delivery = CreateDelivery(moisture);

        Assert.Equal(0m, delivery.ExtraGasVolume);
        Assert.False(delivery.RequiresThermalCompensation);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ABC123")]
    [InlineData("")]
    public void Constructor_WithInvalidPlate_Throws(string plate)
    {
        Assert.Throws<ArgumentException>(() =>
            new WoodDelivery(plate, "Origin", "Species", 50m, 10m, 45m));
    }

    [Theory]
    [InlineData(0, 0, 45)]
    [InlineData(50, 50, 45)]
    [InlineData(50, -1, 45)]
    [InlineData(50, 10, 9)]
    [InlineData(50, 10, 71)]
    public void Constructor_WithInconsistentMeasurements_Throws(
        decimal grossWeight,
        decimal tareWeight,
        decimal moisture)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WoodDelivery("ABC1D23", "Origin", "Species", grossWeight, tareWeight, moisture));
    }

    [Fact]
    public void ChangeStatus_WithKnownStatus_UpdatesStatus()
    {
        var delivery = CreateDelivery(45m);

        delivery.ChangeStatus(DeliveryStatus.Unloading);

        Assert.Equal(DeliveryStatus.Unloading, delivery.Status);
    }

    [Fact]
    public void PublicProperties_DoNotExposeSetters()
    {
        var writableProperties = typeof(WoodDelivery)
            .GetProperties()
            .Where(property => property.SetMethod?.IsPublic == true);

        Assert.Empty(writableProperties);
    }

    private static WoodDelivery CreateDelivery(decimal moisture) =>
        new("ABC1D23", "Origin", "Species", 50m, 10m, moisture);
}

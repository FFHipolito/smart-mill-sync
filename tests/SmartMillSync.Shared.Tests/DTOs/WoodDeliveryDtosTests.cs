using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Shared.Tests.DTOs;

public sealed class WoodDeliveryDtosTests
{
    [Fact]
    public void Contracts_AreImmutableRecords()
    {
        AssertRecord<CreateWoodDeliveryRequest>();
        AssertRecord<WoodDeliveryResponse>();
        AssertRecord<EnergyBalanceSummaryDto>();
    }

    [Fact]
    public void Enums_HaveStableContractValues()
    {
        Assert.Equal(1, (int)DeliveryStatus.InTransit);
        Assert.Equal(6, (int)DeliveryStatus.Rejected);
        Assert.Equal(1, (int)AlertLevel.Normal);
        Assert.Equal(3, (int)AlertLevel.High);
    }

    private static void AssertRecord<T>()
    {
        var equalityContract = typeof(T).GetProperty(
            "EqualityContract",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(equalityContract);
        Assert.All(typeof(T).GetProperties(), property =>
        {
            var setter = property.SetMethod;
            Assert.NotNull(setter);
            Assert.Contains(
                typeof(System.Runtime.CompilerServices.IsExternalInit),
                setter.ReturnParameter.GetRequiredCustomModifiers());
        });
    }
}

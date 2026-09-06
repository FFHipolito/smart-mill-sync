using SmartMillSync.Domain.Enums;
using Xunit;

namespace SmartMillSync.Domain.Tests.Enums;

public sealed class DeliveryStatusTests
{
    [Fact]
    public void Values_AreStableForPersistenceAndContracts()
    {
        Assert.Equal(1, (int)DeliveryStatus.InTransit);
        Assert.Equal(2, (int)DeliveryStatus.ArrivedAtGate);
        Assert.Equal(3, (int)DeliveryStatus.Weighed);
        Assert.Equal(4, (int)DeliveryStatus.Unloading);
        Assert.Equal(5, (int)DeliveryStatus.Completed);
        Assert.Equal(6, (int)DeliveryStatus.Rejected);
    }
}

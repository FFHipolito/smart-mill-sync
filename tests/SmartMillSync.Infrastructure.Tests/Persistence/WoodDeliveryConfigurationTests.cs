using Microsoft.EntityFrameworkCore;
using SmartMillSync.Domain.Entities;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Persistence;

public sealed class WoodDeliveryConfigurationTests
{
    [Fact]
    public void Model_ConfiguresTableKeyPrecisionAndCalculatedProperties()
    {
        using var context = SmartMillDbContextTests.CreateContext();
        var entity = context.Model.FindEntityType(typeof(WoodDelivery));

        Assert.NotNull(entity);
        Assert.Equal("wood_deliveries", entity.GetTableName());
        Assert.Equal(nameof(WoodDelivery.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(18, entity.FindProperty(nameof(WoodDelivery.GrossWeight))!.GetPrecision());
        Assert.Equal(2, entity.FindProperty(nameof(WoodDelivery.GrossWeight))!.GetScale());
        Assert.Null(entity.FindProperty(nameof(WoodDelivery.NetWeight)));
        Assert.Null(entity.FindProperty(nameof(WoodDelivery.ExtraGasVolume)));
    }
}

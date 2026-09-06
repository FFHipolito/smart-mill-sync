using SmartMillSync.Application.Abstractions;
using Xunit;

namespace SmartMillSync.Application.Tests.Abstractions;

public sealed class IWoodDeliveryRepositoryTests
{
    [Fact]
    public void Contract_ExposesOnlyAsynchronousOperationsWithCancellation()
    {
        var methods = typeof(IWoodDeliveryRepository).GetMethods();

        Assert.Equal(3, methods.Length);
        Assert.All(methods, method =>
        {
            Assert.True(typeof(Task).IsAssignableFrom(method.ReturnType));
            Assert.Equal(typeof(CancellationToken), method.GetParameters()[^1].ParameterType);
        });
    }
}

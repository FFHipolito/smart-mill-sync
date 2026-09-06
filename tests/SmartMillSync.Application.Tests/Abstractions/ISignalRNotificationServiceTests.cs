using SmartMillSync.Application.Abstractions;
using Xunit;

namespace SmartMillSync.Application.Tests.Abstractions;

public sealed class ISignalRNotificationServiceTests
{
    [Fact]
    public void Contract_ExposesTypedAsynchronousNotifications()
    {
        var methods = typeof(ISignalRNotificationService).GetMethods();

        Assert.Equal(2, methods.Length);
        Assert.All(methods, method =>
        {
            Assert.Equal(typeof(Task), method.ReturnType);
            Assert.Equal(typeof(CancellationToken), method.GetParameters()[^1].ParameterType);
        });
    }
}

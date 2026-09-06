using SmartMillSync.Application.Agents;
using Xunit;

namespace SmartMillSync.Application.Tests.Agents;

public sealed class IIndustrialAgentServiceTests
{
    [Fact]
    public void Contract_ExposesChatAndDeliveryDiagnosisWithCancellation()
    {
        var methods = typeof(IIndustrialAgentService).GetMethods();

        Assert.Equal(2, methods.Length);
        Assert.All(methods, method =>
        {
            Assert.True(typeof(Task).IsAssignableFrom(method.ReturnType));
            Assert.Equal(typeof(CancellationToken), method.GetParameters()[^1].ParameterType);
        });
    }
}

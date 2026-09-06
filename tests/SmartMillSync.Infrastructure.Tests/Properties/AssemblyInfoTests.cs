using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace SmartMillSync.Infrastructure.Tests.Properties;

public sealed class AssemblyInfoTests
{
    [Fact]
    public void Assembly_ExposesInternalsOnlyToInfrastructureTests()
    {
        var attributes = typeof(Infrastructure.Workers.ThermalBalanceWorker).Assembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName);

        Assert.Contains("SmartMillSync.Infrastructure.Tests", attributes);
    }
}

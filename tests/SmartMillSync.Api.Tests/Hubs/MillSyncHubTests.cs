using Microsoft.AspNetCore.SignalR;
using SmartMillSync.Api.Hubs;
using Xunit;

namespace SmartMillSync.Api.Tests.Hubs;

public sealed class MillSyncHubTests
{
    [Fact]
    public void Hub_IsConnectionOnlyAndDoesNotExposeBusinessCommands()
    {
        Assert.True(typeof(Hub).IsAssignableFrom(typeof(MillSyncHub)));
        Assert.DoesNotContain(
            typeof(MillSyncHub).GetMethods(),
            method => method.DeclaringType == typeof(MillSyncHub));
    }
}

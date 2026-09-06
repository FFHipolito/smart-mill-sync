using SmartMillSync.Client.Services;
using Xunit;

namespace SmartMillSync.Client.Tests.Services;

public sealed class MillRealtimeClientTests
{
    [Fact]
    public void Client_ImplementsTypedContract()
    {
        Assert.True(typeof(IMillRealtimeClient).IsAssignableFrom(typeof(MillRealtimeClient)));
    }

    [Fact]
    public async Task Constructor_WithApiBaseAddress_CreatesDisposableClient()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5243/") };

        await using var client = new MillRealtimeClient(httpClient);

        Assert.IsAssignableFrom<IAsyncDisposable>(client);
    }

    [Fact]
    public void Constructor_WithoutBaseAddress_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MillRealtimeClient(new HttpClient()));
    }
}

using SmartMillSync.Api.Services;
using Xunit;

namespace SmartMillSync.Api.Tests.Services;

public sealed class UnavailableIndustrialAgentChatGatewayTests
{
    [Fact]
    public async Task CompleteAsync_ExplainsRequiredGeminiConfiguration()
    {
        var gateway = new UnavailableIndustrialAgentChatGateway();

        var exception = await Assert.ThrowsAsync<IndustrialAgentUnavailableException>(() =>
            gateway.CompleteAsync("system", [], "message", CancellationToken.None));

        Assert.Contains("GOOGLE_AI_API_KEY", exception.Message);
    }
}

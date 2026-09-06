using SmartMillSync.Application.Agents;
using Xunit;

namespace SmartMillSync.Application.Tests.Agents;

public sealed class SemanticKernelChatGatewayTests
{
    [Fact]
    public void Gateway_ImplementsAgentChatContract()
    {
        Assert.True(typeof(IIndustrialAgentChatGateway).IsAssignableFrom(typeof(SemanticKernelChatGateway)));
    }
}

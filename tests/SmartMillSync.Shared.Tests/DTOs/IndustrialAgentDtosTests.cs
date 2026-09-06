using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Shared.Tests.DTOs;

public sealed class IndustrialAgentDtosTests
{
    [Fact]
    public void Contracts_AreImmutableRecords()
    {
        AssertRecord<AgentChatMessageDto>();
        AssertRecord<IndustrialAgentChatRequest>();
        AssertRecord<IndustrialAgentResponse>();
    }

    [Fact]
    public void Roles_HaveStableContractValues()
    {
        Assert.Equal(1, (int)AgentMessageRole.User);
        Assert.Equal(2, (int)AgentMessageRole.Assistant);
    }

    private static void AssertRecord<T>()
    {
        var equalityContract = typeof(T).GetProperty(
            "EqualityContract",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(equalityContract);
        Assert.All(typeof(T).GetProperties(), property =>
        {
            var setter = property.SetMethod;
            Assert.NotNull(setter);
            Assert.Contains(
                typeof(System.Runtime.CompilerServices.IsExternalInit),
                setter.ReturnParameter.GetRequiredCustomModifiers());
        });
    }
}

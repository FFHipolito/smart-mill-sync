using SmartMillSync.Application.Agents;
using Xunit;

namespace SmartMillSync.Application.Tests.Agents;

public sealed class IndustrialAgentOptionsTests
{
    [Fact]
    public void Defaults_UseGeminiFlashAndConfiguredGasPriceBaseline()
    {
        var options = new IndustrialAgentOptions();

        Assert.Equal("IndustrialAgent", IndustrialAgentOptions.SectionName);
        Assert.Equal("gemini-1.5-flash", options.ModelId);
        Assert.Equal(2.50m, options.GasPricePerNm3);
        Assert.Empty(options.ApiKey);
    }
}

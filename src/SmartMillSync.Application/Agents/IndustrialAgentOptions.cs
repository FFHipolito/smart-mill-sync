namespace SmartMillSync.Application.Agents;

public sealed class IndustrialAgentOptions
{
    public const string SectionName = "IndustrialAgent";

    public string ModelId { get; init; } = "gemini-2.0-flash";
    public string ApiKey { get; init; } = string.Empty;
    public decimal GasPricePerNm3 { get; init; } = 2.50m;
}

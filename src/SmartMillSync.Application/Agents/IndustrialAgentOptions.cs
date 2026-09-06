namespace SmartMillSync.Application.Agents;

public sealed class IndustrialAgentOptions
{
    public const string SectionName = "IndustrialAgent";

    public string ModelId { get; set; } = "gemini-1.5-flash";
    public string ApiKey { get; set; } = string.Empty;
    public decimal GasPricePerNm3 { get; set; } = 2.50m;
}

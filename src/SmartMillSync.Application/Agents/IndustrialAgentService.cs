using MediatR;
using Microsoft.Extensions.Options;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Agents;

public static class IndustrialAgentPersona
{
    public const string SystemPrompt = """
        Voce e o Oraculo Industrial da Suzano, assistente especialista em eficiencia energetica e processos de celulose.
        Seu objetivo e analisar dados de pesagem e biomassa provenientes do MLPlan e orientar os operadores do patio e digestor
        para minimizar a queima complementar de Gas Natural (GN). Seja analitico, tecnico, direto e sugira acoes operacionais claras,
        como alocar madeira umida em pilhas de secagem natural. Use obrigatoriamente as ferramentas de telemetria para afirmar dados
        operacionais ou calcular impacto financeiro. Nunca invente cargas, medicoes, custos ou estado da caldeira. Quando nao houver
        dados suficientes, informe a limitacao de forma objetiva.
        """;
}

public interface IIndustrialAgentChatGateway
{
    Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<AgentChatMessageDto> history,
        string message,
        CancellationToken cancellationToken);
}

public sealed class IndustrialAgentService(
    IIndustrialAgentChatGateway chatGateway,
    ISender sender,
    IOptions<IndustrialAgentOptions> options)
    : IIndustrialAgentService
{
    private const int MaximumHistoryMessages = 20;
    private readonly IndustrialAgentOptions _options = options.Value;

    public async Task<IndustrialAgentResponse> ChatAsync(
        IndustrialAgentChatRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Operator message is required.", nameof(request));
        }

        var history = (request.History ?? [])
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .TakeLast(MaximumHistoryMessages)
            .ToArray();
        var analysis = await chatGateway.CompleteAsync(
            IndustrialAgentPersona.SystemPrompt,
            history,
            request.Message.Trim(),
            cancellationToken);

        return new IndustrialAgentResponse(
            analysis,
            DateTimeOffset.UtcNow,
            _options.ModelId);
    }

    public async Task<IndustrialAgentResponse> DiagnoseDeliveryAsync(
        Guid deliveryId,
        CancellationToken cancellationToken)
    {
        var deliveries = await sender.Send(new GetActiveDeliveriesQuery(), cancellationToken);
        var delivery = deliveries.SingleOrDefault(item => item.Id == deliveryId)
            ?? throw new KeyNotFoundException($"Active delivery '{deliveryId}' was not found.");
        var prompt = $"""
            Diagnostique a carga ativa {delivery.TruckPlate}, identificador {delivery.Id}.
            Confirme os dados usando as ferramentas e avalie umidade de {delivery.MoisturePercentage:N2}%
            para {delivery.NetWeight:N2} toneladas liquidas. Informe impacto em GN, custo estimado,
            nivel de risco e a acao operacional recomendada.
            """;
        var analysis = await chatGateway.CompleteAsync(
            IndustrialAgentPersona.SystemPrompt,
            [],
            prompt,
            cancellationToken);

        return new IndustrialAgentResponse(
            analysis,
            DateTimeOffset.UtcNow,
            _options.ModelId,
            deliveryId);
    }
}

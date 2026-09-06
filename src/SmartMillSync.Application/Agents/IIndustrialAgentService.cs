using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Agents;

public interface IIndustrialAgentService
{
    Task<IndustrialAgentResponse> ChatAsync(
        IndustrialAgentChatRequest request,
        CancellationToken cancellationToken);

    Task<IndustrialAgentResponse> DiagnoseDeliveryAsync(
        Guid deliveryId,
        CancellationToken cancellationToken);
}

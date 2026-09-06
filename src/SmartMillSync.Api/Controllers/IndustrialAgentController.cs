using Microsoft.AspNetCore.Mvc;
using SmartMillSync.Application.Agents;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Controllers;

[ApiController]
[Route("api/v1/agent")]
public sealed class IndustrialAgentController(IIndustrialAgentService agentService)
    : ControllerBase
{
    [HttpPost("chat")]
    [ProducesResponseType<IndustrialAgentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IndustrialAgentResponse>> Chat(
        IndustrialAgentChatRequest request,
        CancellationToken cancellationToken) =>
        Ok(await agentService.ChatAsync(request, cancellationToken));

    [HttpPost("diagnose-delivery/{id:guid}")]
    [ProducesResponseType<IndustrialAgentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IndustrialAgentResponse>> DiagnoseDelivery(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await agentService.DiagnoseDeliveryAsync(id, cancellationToken));
}

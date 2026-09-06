using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartMillSync.Application.Agents;
using SmartMillSync.Shared.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartMillSync.Api.Controllers;

[ApiController]
[Route("api/v1/agent")]
[Produces("application/json")]
public sealed class IndustrialAgentController(
    IIndustrialAgentService agentService,
    IValidator<IndustrialAgentChatRequest> chatValidator)
    : ControllerBase
{
    [HttpPost("chat", Name = "ChatWithIndustrialAgent")]
    [EnableRateLimiting("agent")]
    [RequestSizeLimit(64 * 1024)]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Chat with the industrial diagnostic agent",
        Description = "Analyzes yard and energy telemetry using Gemini function calling. The message is limited to 2,000 characters and history to 20 messages.",
        OperationId = "ChatWithIndustrialAgent",
        Tags = ["Industrial Agent"])]
    [ProducesResponseType<IndustrialAgentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IndustrialAgentResponse>> Chat(
        [FromBody] IndustrialAgentChatRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await chatValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(
                validation.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            });
        }

        return Ok(await agentService.ChatAsync(request, cancellationToken));
    }

    [HttpPost("diagnose-delivery/{id:guid}", Name = "DiagnoseWoodDelivery")]
    [EnableRateLimiting("agent")]
    [SwaggerOperation(
        Summary = "Diagnose a high-moisture delivery",
        Description = "Generates a grounded diagnosis for an active delivery using live telemetry, estimated GN impact and configured gas price.",
        OperationId = "DiagnoseWoodDelivery",
        Tags = ["Industrial Agent"])]
    [ProducesResponseType<IndustrialAgentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IndustrialAgentResponse>> DiagnoseDelivery(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await agentService.DiagnoseDeliveryAsync(id, cancellationToken));
}

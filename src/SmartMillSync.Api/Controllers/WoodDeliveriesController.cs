using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Shared.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartMillSync.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public sealed class WoodDeliveriesController(ISender sender) : ControllerBase
{
    [HttpPost("deliveries", Name = "RegisterWoodDelivery")]
    [EnableRateLimiting("writes")]
    [RequestSizeLimit(16 * 1024)]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Register a wood delivery",
        Description = "Registers gate measurements. Tare must be lower than gross weight; moisture must be between 10% and 70%.",
        OperationId = "RegisterWoodDelivery",
        Tags = ["Wood Deliveries"])]
    [ProducesResponseType<WoodDeliveryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WoodDeliveryResponse>> Register(
        [FromBody] CreateWoodDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new RegisterWoodDeliveryCommand(request),
            cancellationToken);

        return Created($"/api/v1/deliveries/{response.Id}", response);
    }

    [HttpGet("deliveries", Name = "GetActiveWoodDeliveries")]
    [EnableRateLimiting("reads")]
    [SwaggerOperation(
        Summary = "List active wood deliveries",
        Description = "Returns non-completed and non-rejected deliveries ordered from newest to oldest.",
        OperationId = "GetActiveWoodDeliveries",
        Tags = ["Wood Deliveries"])]
    [ProducesResponseType<IReadOnlyList<WoodDeliveryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<WoodDeliveryResponse>>> GetActive(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetActiveDeliveriesQuery(), cancellationToken));

    [HttpGet("energy-balance", Name = "GetMillEnergyBalance")]
    [EnableRateLimiting("reads")]
    [SwaggerOperation(
        Summary = "Get today's mill energy balance",
        Description = "Returns the current UTC-day totals for deliveries, net wood, dry biomass, average moisture and additional GN in Nm3.",
        OperationId = "GetMillEnergyBalance",
        Tags = ["Energy Balance"])]
    [ProducesResponseType<EnergyBalanceSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnergyBalanceSummaryDto>> GetEnergyBalance(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetMillEnergyBalanceQuery(), cancellationToken));
}

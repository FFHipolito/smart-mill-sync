using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Application.Deliveries.Queries;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class WoodDeliveriesController(ISender sender) : ControllerBase
{
    [HttpPost("deliveries")]
    [ProducesResponseType<WoodDeliveryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WoodDeliveryResponse>> Register(
        CreateWoodDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new RegisterWoodDeliveryCommand(request),
            cancellationToken);

        return Created($"/api/v1/deliveries/{response.Id}", response);
    }

    [HttpGet("deliveries")]
    [ProducesResponseType<IReadOnlyList<WoodDeliveryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WoodDeliveryResponse>>> GetActive(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetActiveDeliveriesQuery(), cancellationToken));

    [HttpGet("energy-balance")]
    [ProducesResponseType<EnergyBalanceSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnergyBalanceSummaryDto>> GetEnergyBalance(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetMillEnergyBalanceQuery(), cancellationToken));
}

using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SmartMillSync.Api.Controllers;
using SmartMillSync.Application.Agents;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Api.Tests.Controllers;

public sealed class IndustrialAgentControllerTests
{
    [Fact]
    public async Task Chat_DelegatesRequestAndReturnsOk()
    {
        var service = Substitute.For<IIndustrialAgentService>();
        var request = new IndustrialAgentChatRequest("Avalie o patio", []);
        var response = new IndustrialAgentResponse(
            "Operacao estavel.", DateTimeOffset.UtcNow, "gemini-2.0-flash");
        service.ChatAsync(request, Arg.Any<CancellationToken>()).Returns(response);
        var controller = new IndustrialAgentController(service);

        var result = await controller.Chat(request, CancellationToken.None);

        Assert.Equal(response, Assert.IsType<OkObjectResult>(result.Result).Value);
        await service.Received(1).ChatAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task DiagnoseDelivery_DelegatesIdAndReturnsOk()
    {
        var service = Substitute.For<IIndustrialAgentService>();
        var deliveryId = Guid.NewGuid();
        var response = new IndustrialAgentResponse(
            "Enviar para secagem.", DateTimeOffset.UtcNow, "gemini-2.0-flash", deliveryId);
        service.DiagnoseDeliveryAsync(deliveryId, Arg.Any<CancellationToken>()).Returns(response);
        var controller = new IndustrialAgentController(service);

        var result = await controller.DiagnoseDelivery(deliveryId, CancellationToken.None);

        Assert.Equal(response, Assert.IsType<OkObjectResult>(result.Result).Value);
        await service.Received(1).DiagnoseDeliveryAsync(deliveryId, CancellationToken.None);
    }
}

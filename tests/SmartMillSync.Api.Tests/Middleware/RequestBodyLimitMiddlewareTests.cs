using Microsoft.AspNetCore.Http;
using SmartMillSync.Api.Middleware;
using Xunit;

namespace SmartMillSync.Api.Tests.Middleware;

public sealed class RequestBodyLimitMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenContentLengthExceedsLimit_ReturnsProblemDetails()
    {
        var nextCalled = false;
        var middleware = new RequestBodyLimitMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/agent/chat";
        context.Request.ContentLength = RequestBodyLimitMiddleware.AgentBodyLimit + 1;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Contains("Request body too large", body);
        Assert.False(nextCalled);
    }
}

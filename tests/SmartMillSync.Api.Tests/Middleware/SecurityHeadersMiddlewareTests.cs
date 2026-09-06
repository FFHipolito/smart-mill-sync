using Microsoft.AspNetCore.Http;
using SmartMillSync.Api.Middleware;
using Xunit;

namespace SmartMillSync.Api.Tests.Middleware;

public sealed class SecurityHeadersMiddlewareTests
{
    [Theory]
    [InlineData("/api/v1/deliveries", "default-src 'none'; frame-ancestors 'none'")]
    [InlineData("/swagger/index.html", "default-src 'self'")]
    public async Task InvokeAsync_AddsExpectedSecurityHeaders(string path, string expectedCsp)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        Assert.Equal("nosniff", context.Response.Headers.XContentTypeOptions);
        Assert.Equal("DENY", context.Response.Headers.XFrameOptions);
        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
        Assert.Contains(expectedCsp, context.Response.Headers.ContentSecurityPolicy.ToString());
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }
}

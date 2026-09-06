using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartMillSync.Api.Errors;
using SmartMillSync.Api.Services;
using Xunit;

namespace SmartMillSync.Api.Tests.Errors;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithValidationFailure_WritesBadRequestProblem()
    {
        var problemDetails = Substitute.For<IProblemDetailsService>();
        ProblemDetailsContext? writtenContext = null;
        problemDetails.TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => writtenContext = context))
            .Returns(ValueTask.FromResult(true));
        var handler = new GlobalExceptionHandler(
            problemDetails,
            NullLogger<GlobalExceptionHandler>.Instance);
        var exception = new ValidationException(
            new[] { new FluentValidation.Results.ValidationFailure("TruckPlate", "Invalid plate.") });
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.Equal("Validation failed", writtenContext!.ProblemDetails.Title);
        Assert.True(writtenContext.ProblemDetails.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public async Task TryHandleAsync_WhenGeminiIsNotConfigured_WritesServiceUnavailable()
    {
        var problemDetails = Substitute.For<IProblemDetailsService>();
        ProblemDetailsContext? writtenContext = null;
        problemDetails.TryWriteAsync(Arg.Do<ProblemDetailsContext>(context => writtenContext = context))
            .Returns(ValueTask.FromResult(true));
        var handler = new GlobalExceptionHandler(
            problemDetails,
            NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new IndustrialAgentUnavailableException("Gemini unavailable."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
        Assert.Equal("Industrial agent unavailable", writtenContext!.ProblemDetails.Title);
    }
}

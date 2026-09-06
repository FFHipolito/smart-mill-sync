using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartMillSync.Api.Services;

namespace SmartMillSync.Api.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, extensions) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray())
                }),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                new Dictionary<string, object?>()),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                new Dictionary<string, object?>()),
            IndustrialAgentUnavailableException => (
                StatusCodes.Status503ServiceUnavailable,
                "Industrial agent unavailable",
                new Dictionary<string, object?>()),
            GeminiRequestException geminiException when
                geminiException.StatusCode is System.Net.HttpStatusCode.TooManyRequests or
                    System.Net.HttpStatusCode.ServiceUnavailable => (
                StatusCodes.Status503ServiceUnavailable,
                "Gemini temporarily unavailable",
                new Dictionary<string, object?>()),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                new Dictionary<string, object?>())
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception.");
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Extensions = extensions
            },
            Exception = exception
        });
    }
}

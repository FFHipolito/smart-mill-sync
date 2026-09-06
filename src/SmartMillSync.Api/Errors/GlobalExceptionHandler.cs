using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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

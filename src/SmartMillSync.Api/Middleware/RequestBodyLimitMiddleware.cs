using Microsoft.AspNetCore.Mvc;

namespace SmartMillSync.Api.Middleware;

public sealed class RequestBodyLimitMiddleware(RequestDelegate next)
{
    public const long AgentBodyLimit = 64 * 1024;
    public const long DeliveryBodyLimit = 16 * 1024;

    public async Task InvokeAsync(HttpContext context)
    {
        var limit = ResolveLimit(context.Request.Path, context.Request.Method);
        if (limit is not null && context.Request.ContentLength > limit)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "Request body too large",
                Detail = $"The request body cannot exceed {limit.Value / 1024} KB."
            });
            return;
        }

        await next(context);
    }

    internal static long? ResolveLimit(PathString path, string method)
    {
        if (!HttpMethods.IsPost(method))
        {
            return null;
        }

        if (path.StartsWithSegments("/api/v1/agent/chat"))
        {
            return AgentBodyLimit;
        }

        return path.StartsWithSegments("/api/v1/deliveries")
            ? DeliveryBodyLimit
            : null;
    }
}

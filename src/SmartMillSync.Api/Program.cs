using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartMillSync.Api.Configuration;
using SmartMillSync.Api.Errors;
using SmartMillSync.Api.Health;
using SmartMillSync.Api.Hubs;
using SmartMillSync.Api.Middleware;
using SmartMillSync.Api.OpenApi;
using SmartMillSync.Api.Services;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Behaviors;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Infrastructure.Persistence;
using SmartMillSync.Infrastructure.Persistence.Repositories;
using SmartMillSync.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 128 * 1024);

var authenticationEnabled = builder.Services.AddApiSecurity(builder.Configuration, builder.Environment);
if (!builder.Environment.IsDevelopment())
{
    var allowedHosts = builder.Configuration["AllowedHosts"];
    if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts == "*")
    {
        throw new InvalidOperationException("AllowedHosts must list explicit production hostnames.");
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Mill Sync API",
        Version = "v1",
        Description = "Industrial wood-yard telemetry, energy balance and Gemini-assisted diagnostics. All timestamps use UTC and gas volumes use Nm3."
    });
    options.SchemaFilter<SmartMillSchemaFilter>();
    options.OperationFilter<StandardErrorResponsesOperationFilter>(authenticationEnabled);
    if (authenticationEnabled)
    {
        options.AddBearerSecurity();
    }
});
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        if (!IPAddress.TryParse(proxy, out var address))
        {
            throw new InvalidOperationException($"ForwardedHeaders:KnownProxies contains invalid IP '{proxy}'.");
        }

        options.KnownProxies.Add(address);
    }
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:5080", "http://127.0.0.1:5080"];
}
else if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("At least one Cors:AllowedOrigins entry is required outside Development.");
}
builder.Services.AddCors(options => options.AddPolicy("BlazorClient", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Request rate limit exceeded"
        }, cancellationToken);
    };
    options.AddPolicy("agent", context => CreateFixedWindowPartition(context, 6));
    options.AddPolicy("writes", context => CreateFixedWindowPartition(context, 20));
    options.AddPolicy("reads", context => CreateFixedWindowPartition(context, 120));
    options.AddPolicy("readiness", context => CreateFixedWindowPartition(context, 30));
});

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<RegisterWoodDeliveryCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<RegisterWoodDeliveryCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddIndustrialAgent(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("SmartMillDb")
    ?? throw new InvalidOperationException("Connection string 'SmartMillDb' is required.");
builder.Services.AddDbContext<SmartMillDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IWoodDeliveryRepository, WoodDeliveryRepository>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddHostedService<ThermalBalanceWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartMillDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestBodyLimitMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Smart Mill Sync API";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
if (authenticationEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
app.UseRateLimiter();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
    .AllowAnonymous()
    .DisableRateLimiting();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
})
    .AllowAnonymous()
    .RequireRateLimiting("readiness");
app.MapControllers();
app.MapHub<MillSyncHub>("/hubs/mill-sync").RequireRateLimiting("reads");

app.Run();

static RateLimitPartition<string> CreateFixedWindowPartition(HttpContext context, int permitLimit)
{
    var subject = context.User.FindFirstValue("sub")
        ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.User.Identity?.Name;
    var issuer = context.User.FindFirstValue("iss") ?? "local";
    var partitionKey = subject is not null
        ? $"user:{issuer}|{subject}"
        : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0,
        AutoReplenishment = true
    });
}

public partial class Program;


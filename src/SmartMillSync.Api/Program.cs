using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartMillSync.Api.Errors;
using SmartMillSync.Api.Hubs;
using SmartMillSync.Api.Services;
using SmartMillSync.Application.Abstractions;
using SmartMillSync.Application.Behaviors;
using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Infrastructure.Persistence;
using SmartMillSync.Infrastructure.Persistence.Repositories;
using SmartMillSync.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCors(options => options.AddPolicy("BlazorClient", policy =>
    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<RegisterWoodDeliveryCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<RegisterWoodDeliveryCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var connectionString = builder.Configuration.GetConnectionString("SmartMillDb")
    ?? throw new InvalidOperationException("Connection string 'SmartMillDb' is required.");
builder.Services.AddDbContext<SmartMillDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IWoodDeliveryRepository, WoodDeliveryRepository>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddHostedService<ThermalBalanceWorker>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartMillDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.MapControllers();
app.MapHub<MillSyncHub>("/hubs/mill-sync");

app.Run();

public partial class Program;


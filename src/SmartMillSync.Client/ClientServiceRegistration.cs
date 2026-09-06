using Microsoft.Extensions.DependencyInjection;
using SmartMillSync.Client.Services;

namespace SmartMillSync.Client;

public static class ClientServiceRegistration
{
    public static IServiceCollection AddSmartMillClient(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
        services.AddScoped<MillApiClient>();
        services.AddScoped<IMillApiClient>(provider => provider.GetRequiredService<MillApiClient>());
        services.AddScoped<MillRealtimeClient>();
        services.AddScoped<IMillRealtimeClient>(provider => provider.GetRequiredService<MillRealtimeClient>());
        return services;
    }
}

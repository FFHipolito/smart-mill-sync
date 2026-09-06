using SmartMillSync.Api.Services;
using SmartMillSync.Application.Agents;
using SmartMillSync.Application.Plugins;

namespace SmartMillSync.Api.Configuration;

public static class IndustrialAgentServiceRegistration
{
    public static IServiceCollection AddIndustrialAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IndustrialAgentOptions>(
            configuration.GetSection(IndustrialAgentOptions.SectionName));
        services.PostConfigure<IndustrialAgentOptions>(options =>
        {
            var environmentApiKey = Environment.GetEnvironmentVariable("GOOGLE_AI_API_KEY");
            if (!string.IsNullOrWhiteSpace(environmentApiKey))
            {
                options.ApiKey = environmentApiKey;
            }
        });
        services.AddScoped<MillTelemetryPlugin>();
        services.AddScoped<IIndustrialAgentService, IndustrialAgentService>();

        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_AI_API_KEY")
            ?? configuration[$"{IndustrialAgentOptions.SectionName}:ApiKey"];
        var modelId = configuration[$"{IndustrialAgentOptions.SectionName}:ModelId"]
            ?? new IndustrialAgentOptions().ModelId;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddScoped<IIndustrialAgentChatGateway, UnavailableIndustrialAgentChatGateway>();
            return services;
        }

        services.AddHttpClient<IIndustrialAgentChatGateway, GeminiRestChatGateway>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
        });
        return services;
    }
}

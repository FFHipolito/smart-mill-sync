using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
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

#pragma warning disable SKEXP0070
        services.AddGoogleAIGeminiChatCompletion(
            modelId: modelId,
            apiKey: apiKey,
            apiVersion: GoogleAIVersion.V1);
#pragma warning restore SKEXP0070
        services.AddScoped(provider => new Kernel(provider));
        services.AddScoped<IIndustrialAgentChatGateway, SemanticKernelChatGateway>();
        return services;
    }
}

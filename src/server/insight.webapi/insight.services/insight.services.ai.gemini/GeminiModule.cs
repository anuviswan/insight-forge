using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Options;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Insight.Services.Ai.Gemini;

public class GeminiModule : IModule
{
    public string ModuleName => "Gemini";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IGeminiApiClient, GeminiApiHttpClient>(client =>
        {
            var baseUrl = configuration["GeminiAgent:BaseUrl"]
                ?? configuration["Antigravity:BaseUrl"]
                ?? "https://generativelanguage.googleapis.com/v1beta/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Bind options from configuration (appsettings.json)
        services.Configure<GeminiAgentOptions>(configuration.GetSection("GeminiAgents"));

        // Register metadata provider. Use YAML as single source of truth for agent definitions.
        services.AddKeyedScoped<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>, YamlAgentMetadataProvider>(ModuleName);

        // Register GeminiAgent implementing specific interfaces following SRP
        services.AddScoped<GeminiAgent>();
        services.AddScoped<IBlogAgent>(sp => sp.GetRequiredService<GeminiAgent>());
        services.AddScoped<IAgentOrchestrator>(sp => sp.GetRequiredService<GeminiAgent>());
    }
}

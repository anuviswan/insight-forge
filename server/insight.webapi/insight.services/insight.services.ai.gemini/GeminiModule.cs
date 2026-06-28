using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insight.Services.Ai.Gemini;

public class GeminiModule : IModule
{
    public string ModuleName => "Gemini";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IAntigravityApiClient, AntigravityApiClient>(client =>
        {
            var baseUrl = configuration["Antigravity:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Bind options from configuration (appsettings.json)
        services.Configure<GeminiAgentOptions>(configuration.GetSection("GeminiAgents"));

        // Register metadata providers and agent. Use YAML provider as the keyed default for Gemini.
        services.AddKeyedScoped<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>, YamlAgentMetadataProvider>(ModuleName);
        services.AddScoped<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>, MarkdownAgentMetadataProvider>();
        services.AddScoped<IAgent, AntigravityAgent>();
    }
}

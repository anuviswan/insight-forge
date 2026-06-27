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

        // Register metadata provider and agent
        services.AddKeyedScoped<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>, FileAgentMetadataProvider>(ModuleName);
        services.AddScoped<IAgent, AntigravityAgent>();
    }
}

using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiApiHttpClient : IGeminiApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiApiHttpClient> _logger;
    private const string BaseAgentModel = "antigravity-preview-05-2026";

    public GeminiApiHttpClient(HttpClient http, IConfiguration config, ILogger<GeminiApiHttpClient> logger)
    {
        _http = http;
        _logger = logger;

        var apiKey = config["GeminiAgent:ApiKey"] ?? config["Antigravity:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            if (!_http.DefaultRequestHeaders.Contains("x-goog-api-key"))
                _http.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
        }
        else
        {
            _logger.LogWarning("Gemini API key not configured. Set 'GeminiAgent:ApiKey' in appsettings.json");
        }
    }

    public async Task<bool> AgentExistsAsync(string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await _http.GetAsync($"agents/{agentId}", cancellationToken);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if agent exists");
            return false;
        }
    }

    public async Task<string?> CreateManagedAgentAsync(string agentId, string systemInstruction, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default)
    {
        var environmentSources = new List<object>();

        if (agentDefinition != null)
        {
            if (!string.IsNullOrWhiteSpace(agentDefinition.AgentsMd))
            {
                environmentSources.Add(new
                {
                    type = "inline",
                    target = ".agents/AGENTS.md",
                    content = agentDefinition.AgentsMd
                });
            }

            if (agentDefinition.Workflows?.Count > 0)
            {
                foreach (var workflow in agentDefinition.Workflows)
                {
                    environmentSources.Add(new
                    {
                        type = "inline",
                        target = $".agents/workflows/{workflow.Name}.yaml",
                        content = workflow.Content
                    });
                }
            }

            if (agentDefinition.Skills?.Count > 0)
            {
                foreach (var skill in agentDefinition.Skills)
                {
                    environmentSources.Add(new
                    {
                        type = "inline",
                        target = $".agents/skills/{skill.Name}/SKILL.md",
                        content = skill.Content
                    });
                }
            }
        }

        // Fail early if environment sources are empty
        if (environmentSources.Count == 0)
        {
            throw new InvalidOperationException(
                $"Cannot create agent '{agentId}': no environment sources. " +
                "Agent definition must include AgentsMd, workflows, and skills.");
        }

        var payload = new
        {
            id = agentId,
            base_agent = BaseAgentModel,
            system_instruction = systemInstruction,
            base_environment = new
            {
                type = "remote",
                sources = environmentSources
            }
        };

        // Detailed logging for debugging
        Console.WriteLine($"\n========== CREATE AGENT REQUEST ==========");
        Console.WriteLine($"Agent ID: {agentId}");
        Console.WriteLine($"Base Agent: {BaseAgentModel}");
        Console.WriteLine($"System Instruction Length: {systemInstruction?.Length ?? 0}");
        Console.WriteLine($"\nAgent Definition:");
        if (agentDefinition != null)
        {
            Console.WriteLine($"  Name: {agentDefinition.Name}");
            Console.WriteLine($"  AgentsMd Present: {!string.IsNullOrWhiteSpace(agentDefinition.AgentsMd)}");
            Console.WriteLine($"  AgentsMd Length: {agentDefinition.AgentsMd?.Length ?? 0}");
            Console.WriteLine($"  Workflows: {agentDefinition.Workflows?.Count ?? 0}");
            if (agentDefinition.Workflows?.Count > 0)
            {
                foreach (var wf in agentDefinition.Workflows)
                    Console.WriteLine($"    - {wf.Name} ({wf.Content?.Length ?? 0} chars)");
            }
            Console.WriteLine($"  Skills: {agentDefinition.Skills?.Count ?? 0}");
            if (agentDefinition.Skills?.Count > 0)
            {
                foreach (var skill in agentDefinition.Skills)
                    Console.WriteLine($"    - {skill.Name} ({skill.Content?.Length ?? 0} chars)");
            }
        }
        else
        {
            Console.WriteLine("  NULL - No agent definition provided!");
        }
        Console.WriteLine($"\nEnvironment Sources: {environmentSources.Count}");
        Console.WriteLine($"=========================================\n");

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsJsonAsync("agents", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create managed agent via Gemini API");
            throw;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gemini API returned {Status}: {Body}", resp.StatusCode, body);
            throw new HttpRequestException($"Gemini API returned {resp.StatusCode}");
        }

        var response = await resp.Content.ReadFromJsonAsync<GeminiAgentResponse>(cancellationToken: cancellationToken);
        return response?.Id ?? agentId;
    }

    public async Task<string?> RunAgentInteractionAsync(string agentId, string input, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            agent = agentId,
            input = input
        };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsJsonAsync("interactions", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run agent interaction");
            throw;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gemini API returned {Status}: {Body}", resp.StatusCode, body);
            throw new HttpRequestException($"Gemini API returned {resp.StatusCode}");
        }

        var response = await resp.Content.ReadFromJsonAsync<GeminiInteractionResponse>(cancellationToken: cancellationToken);

        if (response?.Steps == null || response.Steps.Count == 0)
            return null;

        // Extract output from model_output step (the final step with actual output)
        var modelOutputStep = response.Steps.LastOrDefault(s => s.Type == "model_output");
        if (modelOutputStep?.Content != null && modelOutputStep.Content.Count > 0)
        {
            return modelOutputStep.Content
                .Where(c => c.Type == "text" && !string.IsNullOrEmpty(c.Text))
                .Select(c => c.Text)
                .FirstOrDefault();
        }

        return null;
    }
}

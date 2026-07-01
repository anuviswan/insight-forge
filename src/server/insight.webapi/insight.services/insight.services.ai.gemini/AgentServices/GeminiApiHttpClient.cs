using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiApiHttpClient : IGeminiApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiApiHttpClient> _logger;

    public GeminiApiHttpClient(HttpClient http, IConfiguration config, ILogger<GeminiApiHttpClient> logger)
    {
        _http = http;
        _logger = logger;

        var apiKey = config["Antigravity:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            if (!_http.DefaultRequestHeaders.Contains("Authorization"))
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    public async Task<string?> RunAgentWorkflowAsync(string agentName, string workflow, string input, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default)
    {
        var payload = new { agent = agentName, workflow, input, agentDefinition };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsJsonAsync("interactions", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Antigravity API");
            throw;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Antigravity API returned {Status}: {Body}", resp.StatusCode, body);
            throw new HttpRequestException($"Antigravity API returned {resp.StatusCode}");
        }

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = doc.RootElement;

        if (root.TryGetProperty("output", out var outEl) && outEl.ValueKind == JsonValueKind.String)
            return outEl.GetString();

        if (root.TryGetProperty("result", out var resEl) && resEl.ValueKind == JsonValueKind.String)
            return resEl.GetString();

        return root.ToString();
    }

    public async Task<string?> CreateManagedAgentAsync(string agentName, string systemInstruction, string input, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default)
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

        var payload = new
        {
            agent = agentName,
            input = input,
            system_instruction = systemInstruction,
            environment = new
            {
                type = "remote",
                sources = environmentSources
            }
        };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsJsonAsync("interactions", payload, cancellationToken);
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

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = doc.RootElement;

        if (root.TryGetProperty("agent", out var agentEl) && agentEl.ValueKind == JsonValueKind.String)
            return agentEl.GetString();

        if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
            return idEl.GetString();

        return root.ToString();
    }
}

using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class AntigravityApiClient : IGeminiApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AntigravityApiClient> _logger;

    public AntigravityApiClient(HttpClient http, IConfiguration config, ILogger<AntigravityApiClient> logger)
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
}

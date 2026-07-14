using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Options;
using Insight.Services.Ai.Gemini.Resilience;
using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Ai.Gemini.Streaming.Types;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiApiHttpClient : IGeminiApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiApiHttpClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IGeminiRetryPolicy _retryPolicy;
    private readonly IStreamingResilienceMetrics _metrics;
    private readonly StreamingErrorPolicyOptions _streamingOptions;
    private const string BaseAgentModel = "antigravity-preview-05-2026";

    public GeminiApiHttpClient(
        HttpClient http,
        IConfiguration config,
        ILogger<GeminiApiHttpClient> logger,
        ILoggerFactory loggerFactory,
        IGeminiRetryPolicy retryPolicy,
        IStreamingResilienceMetrics metrics,
        IOptions<StreamingErrorPolicyOptions> streamingOptions)
    {
        _http = http;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _retryPolicy = retryPolicy;
        _metrics = metrics;
        _streamingOptions = streamingOptions.Value;

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
            if (!string.IsNullOrWhiteSpace(agentDefinition.Specification))
            {
                environmentSources.Add(new
                {
                    type = "inline",
                    target = ".agents/AGENTS.md",
                    content = agentDefinition.Specification
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
            Console.WriteLine($"  Specification Present: {!string.IsNullOrWhiteSpace(agentDefinition.Specification)}");
            Console.WriteLine($"  Specification Length: {agentDefinition.Specification?.Length ?? 0}");
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

        var correlationId = Guid.NewGuid().ToString("N");
        using var scope = _logger.BeginScope("CorrelationId:{CorrelationId} AgentId:{AgentId}", correlationId, agentId);

        return await _retryPolicy.ExecuteAsync(async () =>
        {
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
                throw new HttpRequestException($"Gemini API returned {resp.StatusCode}", inner: null, statusCode: resp.StatusCode);
            }

            var response = await resp.Content.ReadFromJsonAsync<GeminiAgentResponse>(cancellationToken: cancellationToken);
            return response?.Id ?? agentId;
        }, "CreateManagedAgent", cancellationToken);
    }

    public async Task<string?> RunAgentInteractionAsync(string agentId, string input, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            agent = agentId,
            input = input
        };

        var correlationId = Guid.NewGuid().ToString("N");
        using var scope = _logger.BeginScope("CorrelationId:{CorrelationId} AgentId:{AgentId}", correlationId, agentId);

        return await _retryPolicy.ExecuteAsync(async () =>
        {
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
                throw new HttpRequestException($"Gemini API returned {resp.StatusCode}", inner: null, statusCode: resp.StatusCode);
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
        }, "RunAgentInteraction", cancellationToken);
    }

    /// <summary>
    /// Streams agent interaction events. On a transient interruption mid-stream, this
    /// performs a partial retry: it resumes from the last interaction id seen instead
    /// of restarting the interaction from scratch, up to <see cref="StreamingErrorPolicyOptions.MaxRetries"/>
    /// attempts, bounded overall by <see cref="StreamingErrorPolicyOptions.StreamTimeout"/>.
    /// </summary>
    public async IAsyncEnumerable<GeminiStreamEvent> StreamAgentInteractionAsync(
        string agentId,
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId, nameof(agentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));

        var wrapperLogger = _loggerFactory.CreateLogger<GeminiStreamWrapper>();
        var wrapper = new GeminiStreamWrapper(_http, wrapperLogger);

        var correlationId = Guid.NewGuid().ToString("N");
        using var scope = _logger.BeginScope("CorrelationId:{CorrelationId} AgentId:{AgentId}", correlationId, agentId);

        using var timeoutCts = new CancellationTokenSource(_streamingOptions.StreamTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        string? lastInteractionId = null;
        var attempt = 0;
        var delay = _streamingOptions.InitialRetryDelay;

        while (true)
        {
            var enumerator = wrapper.StreamAsync(agentId, input, lastInteractionId, linkedToken).GetAsyncEnumerator(linkedToken);
            var retryDelay = TimeSpan.Zero;

            try
            {
                while (true)
                {
                    bool hasNext;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync();
                    }
                    catch (Exception ex) when (attempt < _streamingOptions.MaxRetries && TransientErrorClassifier.IsRetryable(ex, cancellationToken))
                    {
                        attempt++;
                        _metrics.RecordError("StreamAgentInteraction", TransientErrorClassifier.Classify(ex));
                        _metrics.RecordRetryAttempt("StreamAgentInteraction");
                        _logger.LogWarning(
                            ex,
                            "Stream interrupted (attempt {Attempt}/{MaxRetries}), resuming interaction {InteractionId} in {Delay}",
                            attempt, _streamingOptions.MaxRetries, lastInteractionId ?? "(new)", delay);

                        retryDelay = delay;
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _streamingOptions.RetryBackoffMultiplier);
                        break;
                    }

                    if (!hasNext)
                    {
                        if (attempt > 0)
                            _metrics.RecordRetrySuccess("StreamAgentInteraction");
                        yield break;
                    }

                    var current = enumerator.Current;
                    if (!string.IsNullOrWhiteSpace(current.Interaction?.Id))
                        lastInteractionId = current.Interaction.Id;

                    yield return current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            await Task.Delay(retryDelay, linkedToken).ConfigureAwait(false);
        }
    }
}

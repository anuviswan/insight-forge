using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Insight.Services.Ai.Gemini.Streaming.Types;
using Insight.Services.Ai.Gemini.Types;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.Streaming;

public class GeminiStreamWrapper
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiStreamWrapper> _logger;

    public GeminiStreamWrapper(HttpClient http, ILogger<GeminiStreamWrapper> logger)
    {
        _http = http;
        _logger = logger;
    }

    public IAsyncEnumerable<GeminiStreamEvent> StreamAsync(
        string agentId,
        string input,
        CancellationToken cancellationToken = default)
        => StreamAsync(agentId, input, previousInteractionId: null, cancellationToken);

    /// <summary>
    /// Streams agent interaction events. When <paramref name="previousInteractionId"/> is
    /// provided, asks the API to resume that interaction (a partial retry) instead of
    /// starting a new one from scratch.
    /// </summary>
    public async IAsyncEnumerable<GeminiStreamEvent> StreamAsync(
        string agentId,
        string input,
        string? previousInteractionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId, nameof(agentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));

        object payload = string.IsNullOrWhiteSpace(previousInteractionId)
            ? new { agent = agentId, input = input }
            : new { agent = agentId, input = input, previous_interaction_id = previousInteractionId };
        HttpResponseMessage resp;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "interactions")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Accept", "text/event-stream");
            resp = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate streaming request to Gemini API");
            throw;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gemini API returned {Status}: {Body}", resp.StatusCode, body);
            throw new HttpRequestException($"Gemini API returned {resp.StatusCode}", inner: null, statusCode: resp.StatusCode);
        }

        using var contentStream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(contentStream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await ReadLineOrNullIfCancelledAsync(reader, cancellationToken);
            if (line == null)
                yield break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            _logger.LogDebug("Raw SSE line received: {Line}", line);

            if (line.StartsWith("data: ", StringComparison.OrdinalIgnoreCase))
                line = line["data: ".Length..];

            GeminiStreamEvent? @event = null;
            try
            {
                @event = JsonSerializer.Deserialize<GeminiStreamEvent>(line);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize streaming event: {Line}", line);
                continue;
            }

            if (@event?.EventType != null)
            {
                yield return @event;
                continue;
            }

            // Some agent responses aren't actually delivered incrementally: instead of a
            // sequence of event_type-tagged frames, the whole completed interaction
            // arrives as a single line matching the non-streaming response shape
            // (id/status/steps). That line deserializes into a GeminiStreamEvent with
            // every property null (event_type included) rather than throwing, so it
            // would otherwise be silently discarded, along with the actual generated
            // content sitting in its model_output step. Detect that shape here and
            // synthesize the equivalent event sequence instead.
            GeminiInteractionResponse? completeResponse = null;
            try
            {
                completeResponse = JsonSerializer.Deserialize<GeminiInteractionResponse>(line);
            }
            catch (JsonException)
            {
                // Not that shape either - fall through to the warning below.
            }

            if (completeResponse?.Steps is { Count: > 0 })
            {
                _logger.LogInformation(
                    "Received a complete (non-incremental) interaction response instead of streaming events; synthesizing equivalent events for interaction {InteractionId}",
                    completeResponse.Id);

                foreach (var synthesized in SynthesizeEvents(completeResponse))
                {
                    yield return synthesized;
                }
                continue;
            }

            _logger.LogWarning("Received streaming event with no event_type and unrecognized shape: {Line}", line);
        }
    }

    /// <summary>
    /// Builds the interaction.created / step.start / step.delta / step.stop /
    /// interaction.completed event sequence a real streaming response would have
    /// produced, from a complete (already-finished) interaction response.
    /// </summary>
    private static IEnumerable<GeminiStreamEvent> SynthesizeEvents(GeminiInteractionResponse response)
    {
        yield return new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = response.Id, Status = "active" }
        };

        var steps = response.Steps ?? [];
        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var stepId = step.Id ?? $"step-{index}";
            var content = step.Content?
                .Select(c => new ContentData { Type = c.Type, Text = c.Text })
                .ToList();

            yield return new GeminiStreamEvent
            {
                EventType = "step.start",
                Step = new StepData { Id = stepId, Type = step.Type, Index = index }
            };

            if (content is { Count: > 0 })
            {
                yield return new GeminiStreamEvent
                {
                    EventType = "step.delta",
                    Step = new StepData { Id = stepId, Type = step.Type, Index = index, Content = content }
                };
            }

            yield return new GeminiStreamEvent
            {
                EventType = "step.stop",
                Step = new StepData { Id = stepId, Type = step.Type, Index = index, Content = content }
            };
        }

        yield return new GeminiStreamEvent
        {
            EventType = "interaction.completed",
            Interaction = new InteractionData
            {
                Id = response.Id,
                Status = response.Status,
                Usage = response.Usage == null ? null : new UsageData
                {
                    TotalTokens = response.Usage.TotalTokens,
                    TotalInputTokens = response.Usage.TotalInputTokens,
                    TotalOutputTokens = response.Usage.TotalOutputTokens,
                    TotalCachedTokens = response.Usage.TotalCachedTokens,
                    TotalThoughtTokens = response.Usage.TotalThoughtTokens
                }
            }
        };
    }

    /// <summary>
    /// Reads the next line, treating cancellation as end-of-stream instead of letting
    /// the enumerator throw, so callers can simply stop consuming an <c>await foreach</c>
    /// by cancelling the token.
    /// </summary>
    private static async Task<string?> ReadLineOrNullIfCancelledAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        try
        {
            return await reader.ReadLineAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}

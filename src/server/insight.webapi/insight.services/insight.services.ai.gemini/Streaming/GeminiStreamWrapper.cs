using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Insight.Services.Ai.Gemini.Streaming.Types;
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

            if (@event != null)
                yield return @event;
        }
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

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

    public async IAsyncEnumerable<GeminiStreamEvent> StreamAsync(
        string agentId,
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId, nameof(agentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));

        var payload = new { agent = agentId, input = input };
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
            throw new HttpRequestException($"Gemini API returned {resp.StatusCode}");
        }

        using var contentStream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(contentStream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
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
}

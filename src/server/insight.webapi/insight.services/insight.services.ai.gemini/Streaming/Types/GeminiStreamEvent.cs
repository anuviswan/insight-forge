using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Streaming.Types;

public class GeminiStreamEvent
{
    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }

    [JsonPropertyName("interaction")]
    public InteractionData? Interaction { get; set; }

    [JsonPropertyName("step")]
    public StepData? Step { get; set; }

    [JsonPropertyName("usage")]
    public UsageData? Usage { get; set; }
}

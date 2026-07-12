using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Streaming.Types;

public class FunctionCallData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("args")]
    public Dictionary<string, object>? Args { get; set; }
}

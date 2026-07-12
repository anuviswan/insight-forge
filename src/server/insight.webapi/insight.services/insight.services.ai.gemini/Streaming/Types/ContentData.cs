using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Streaming.Types;

public class ContentData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("thinking")]
    public string? Thinking { get; set; }
}

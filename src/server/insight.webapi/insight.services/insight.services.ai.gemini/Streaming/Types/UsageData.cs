using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Streaming.Types;

public class UsageData
{
    [JsonPropertyName("total_tokens")]
    public long? TotalTokens { get; set; }

    [JsonPropertyName("total_input_tokens")]
    public long? TotalInputTokens { get; set; }

    [JsonPropertyName("total_output_tokens")]
    public long? TotalOutputTokens { get; set; }

    [JsonPropertyName("total_cached_tokens")]
    public long? TotalCachedTokens { get; set; }

    [JsonPropertyName("total_thought_tokens")]
    public long? TotalThoughtTokens { get; set; }
}

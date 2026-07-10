using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

public class GeminiInteractionResponse
{
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }
}

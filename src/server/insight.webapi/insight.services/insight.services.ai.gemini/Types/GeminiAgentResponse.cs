using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

public class GeminiAgentResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("base_agent")]
    public string? BaseAgent { get; set; }

    [JsonPropertyName("system_instruction")]
    public string? SystemInstruction { get; set; }

    [JsonPropertyName("base_environment")]
    public BaseEnvironment? BaseEnvironment { get; set; }
}

public class BaseEnvironment
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("sources")]
    public List<EnvironmentSource>? Sources { get; set; }
}

public class EnvironmentSource
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

public class GeminiAgentResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("baseAgent")]
    public string? BaseAgent { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    [JsonPropertyName("systemInstruction")]
    public SystemInstruction? SystemInstruction { get; set; }

    [JsonPropertyName("baseEnvironment")]
    public BaseEnvironment? BaseEnvironment { get; set; }
}

public class SystemInstruction
{
    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
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

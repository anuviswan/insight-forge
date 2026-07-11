using System.Text.Json;
using System.Text.Json.Serialization;

namespace Insight.Services.Ai.Gemini.Types;

// Custom converter to handle both array and non-array values for Content/Summary
public class ContentArrayConverter : JsonConverter<List<StepContent>>
{
    public override List<StepContent> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new List<StepContent>();

        if (reader.TokenType == JsonTokenType.Null)
        {
            return result;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return result;
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var content = JsonSerializer.Deserialize<StepContent>(ref reader, options);
                if (content != null)
                    result.Add(content);
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<StepContent>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}

public class GeminiInteractionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("steps")]
    public List<InteractionStep>? Steps { get; set; }

    [JsonPropertyName("usage")]
    public UsageMetadata? Usage { get; set; }

    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("agent")]
    public string? Agent { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }
}

public class InteractionStep
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("content")]
    [JsonConverter(typeof(ContentArrayConverter))]
    public List<StepContent>? Content { get; set; }

    [JsonPropertyName("summary")]
    [JsonConverter(typeof(ContentArrayConverter))]
    public List<StepContent>? Summary { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("arguments")]
    public StepArguments? Arguments { get; set; }

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

public class StepContent
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class StepArguments
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

public class UsageMetadata
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

    [JsonPropertyName("input_tokens_by_modality")]
    public List<ModalityTokens>? InputTokensByModality { get; set; }

    [JsonPropertyName("output_tokens_by_modality")]
    public List<ModalityTokens>? OutputTokensByModality { get; set; }
}

public class ModalityTokens
{
    [JsonPropertyName("modality")]
    public string? Modality { get; set; }

    [JsonPropertyName("tokens")]
    public long? Tokens { get; set; }
}

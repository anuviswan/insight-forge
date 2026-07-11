using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Minimal test to understand the response structure
var sampleResponse = @"{
  ""id"":""v1_ChdjT2RSYXZXYkFxcm5qdU1QMXBiMHlBZxIXY09kUmF2V2JBcXJuanVNUDFwYjB5QWc"",
  ""status"":""completed"",
  ""steps"":[
    {
      ""type"":""thought"",
      ""summary"":[{""text"":""Test"",""type"":""text""}]
    },
    {
      ""type"":""model_output"",
      ""content"":[{""text"":""Blog content here"",""type"":""text""}]
    }
  ]
}";

try
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    var doc = JsonDocument.Parse(sampleResponse);
    Console.WriteLine("Sample parsed successfully!");

    var root = doc.RootElement;
    Console.WriteLine($"Root type: {root.ValueKind}");

    if (root.TryGetProperty("steps", out var stepsElement))
    {
        Console.WriteLine($"Steps type: {stepsElement.ValueKind}");
        if (stepsElement.ValueKind == JsonValueKind.Array)
        {
            Console.WriteLine($"Steps count: {stepsElement.GetArrayLength()}");
            foreach (var step in stepsElement.EnumerateArray())
            {
                Console.WriteLine($"  Step type: {step.ValueKind}");
                if (step.TryGetProperty("type", out var typeEl))
                {
                    Console.WriteLine($"    - type: {typeEl.GetString()}");
                }
                if (step.TryGetProperty("content", out var contentEl))
                {
                    Console.WriteLine($"    - content type: {contentEl.ValueKind}");
                }
                if (step.TryGetProperty("result", out var resultEl))
                {
                    Console.WriteLine($"    - result type: {resultEl.ValueKind}");
                    if (resultEl.ValueKind == JsonValueKind.String)
                    {
                        Console.WriteLine($"    - result is STRING (contains JSON!)");
                    }
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}

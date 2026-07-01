namespace Insight.Services.Ai.Gemini.Options;

public class GeminiAgentOptions
{
    public string ApiKey { get; set; }
    public string AgentsDefinitionFile { get; set; } = "agents";
    public string WorkflowsDefinitionFolder { get; set; } = "workflows";
    public string SkillsDefinitionFolder { get; set; } = "skills";
    public string CreateAgentEndpoint { get; set; } = "interactions";
}

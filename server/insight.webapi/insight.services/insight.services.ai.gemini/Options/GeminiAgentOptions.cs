namespace Insight.Services.Ai.Gemini.Options;

public class GeminiAgentOptions
{
    // Root folder where agent definition folders live (can be absolute or relative to application base)
    public string AgentsDefinitionFile { get; set; } = "agents";
    public string WorkflowsDefinitionFolder { get; set; } = "workflows";

    public string SkillsDefinitionFolder { get; set; } = "skills";

}

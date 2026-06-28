namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgentOptions
{
    // Root folder where agent definition folders live (can be absolute or relative to application base)
    public string AgentRootFolder { get; set; } = "agents";
}

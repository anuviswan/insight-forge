using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class GeminiAgent(IGeminiApiClient apiClient, IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto> metadataProvider) : IAgent
{
    private const string AgentName = "Blog Writer Agent";
    private const string Workflow = "create-blogpost";

    public Task<string> CheckIfAgentExists(string agentName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateAgent(string agentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            throw new ArgumentException("Agent name must be provided", nameof(agentName));

        var agentDef = metadataProvider.GetAgent(agentName);
        if (agentDef == null)
            throw new InvalidOperationException($"Agent definition not found for '{agentName}'");

        var systemInstruction = agentDef.Content ?? $"You are the {agentName} agent.";
        var result = await apiClient.CreateManagedAgentAsync(agentName, systemInstruction, "", agentDef, cancellationToken);
        return result ?? string.Empty;
    }

    public async Task<string> CreateBlogPostAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        var input = BuildPrompt(topic, audience, writingStyle);

        var agentDef = metadataProvider.GetAgent("Antigravity");

        var result = await apiClient.RunAgentWorkflowAsync(AgentName, Workflow, input, agentDef, cancellationToken);
        return result ?? string.Empty;
    }

    private static string BuildPrompt(string topic, string audience, string writingStyle)
    {
        var prompt = $"Write a comprehensive blog post about '{topic}'";

        if (!string.IsNullOrWhiteSpace(audience))
            prompt += $"\n\nIntended Audience: {audience.Trim()}";

        if (!string.IsNullOrWhiteSpace(writingStyle))
            prompt += $"\n\nWriting Style/Tone: {writingStyle.Trim()}";

        prompt += "\n\nProvide the complete blog post in well-formatted Markdown.";

        return prompt;
    }
}

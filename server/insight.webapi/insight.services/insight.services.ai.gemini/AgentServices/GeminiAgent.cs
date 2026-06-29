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

    public Task<string> CreateAgent(string agentName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateBlogPostAsync(string topic, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic must be provided", nameof(topic));

        var input = topic.Trim();

        var agentDef = metadataProvider.GetAgent("Antigravity");

        var result = await apiClient.RunAgentWorkflowAsync(AgentName, Workflow, input, agentDef, cancellationToken);
        return result ?? string.Empty;
    }
}

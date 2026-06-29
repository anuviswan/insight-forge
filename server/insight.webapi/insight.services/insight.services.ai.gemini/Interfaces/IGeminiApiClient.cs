using Insight.Services.Ai.Gemini.Types;

namespace Insight.Services.Ai.Gemini.Interfaces;

public interface IGeminiApiClient
{
    Task<string?> RunAgentWorkflowAsync(string agentName, string workflow, string input, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default);
}

using Insight.Services.Ai.Gemini.Types;

namespace Insight.Services.Ai.Gemini.Interfaces;

public interface IGeminiApiClient
{
    Task<bool> AgentExistsAsync(string agentId, CancellationToken cancellationToken = default);
    Task<string?> CreateManagedAgentAsync(string agentId, string systemInstruction, AgentDefinitionDto? agentDefinition = null, CancellationToken cancellationToken = default);
    Task<string?> RunAgentInteractionAsync(string agentId, string input, CancellationToken cancellationToken = default);
}

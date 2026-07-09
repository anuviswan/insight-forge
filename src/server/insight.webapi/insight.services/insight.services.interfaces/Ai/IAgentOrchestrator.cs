namespace Insight.Services.Interfaces.Ai;

public interface IAgentOrchestrator
{
    Task<string> CreateAgent(string agentName, CancellationToken cancellationToken = default);
    Task<string> CheckIfAgentExists(string agentName, CancellationToken cancellationToken = default);
}

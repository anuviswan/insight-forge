namespace Insight.Services.Interfaces.Ai;

public interface IAgent
{
    Task<string> CreateAgent(string agentName,CancellationToken cancellationToken = default);
    Task<string> CheckIfAgentExists(string agentName, CancellationToken cancellationToken = default);
    Task<string> CreateBlogPostAsync(string topic, CancellationToken cancellationToken = default);
}

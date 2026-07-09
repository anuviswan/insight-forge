using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class ResearchService(IAgent agent) : IResearchService
{
    private readonly IAgent _agent = agent;

    public async Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        return await _agent.ConductResearchAsync(topic, audience, writingStyle, cancellationToken);
    }
}

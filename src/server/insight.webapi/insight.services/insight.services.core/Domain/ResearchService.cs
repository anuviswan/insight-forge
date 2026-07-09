using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class ResearchService(IAgent agent) : IResearchService
{
    public async Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        return await agent.ConductResearchAsync(topic, audience, writingStyle, cancellationToken).ConfigureAwait(false);
    }
}

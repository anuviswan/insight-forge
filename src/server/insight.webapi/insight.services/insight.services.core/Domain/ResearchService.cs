using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class ResearchService(IResearchAgent researchAgent) : IResearchService
{
    public async Task<string> ConductResearchAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        return await researchAgent.ConductResearchAsync(topic, audience, writingStyle, cancellationToken).ConfigureAwait(false);
    }
}

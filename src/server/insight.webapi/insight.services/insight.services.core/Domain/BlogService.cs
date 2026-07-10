using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class BlogService(IBlogAgent blogAgent) : IBlogService
{
    public async Task<string> CreateBlogEntryAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default)
    {
        return await blogAgent.CreateBlogPostAsync(topic, audience, writingStyle, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> CreateBlogEntryWithResearchAsync(string topic, string audience, string writingStyle, string researchArtifacts, CancellationToken cancellationToken = default)
    {
        return await blogAgent.CreateBlogPostWithResearchAsync(topic, audience, writingStyle, researchArtifacts, cancellationToken).ConfigureAwait(false);
    }
}

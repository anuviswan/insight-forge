using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class BlogService(IBlogAgent blogAgent) : IBlogService
{
    public async Task<string> CreateBlogEntryAsync(string topic, string audience, string writingStyle, string researchArtifacts, CancellationToken cancellationToken = default)
    {
        return await blogAgent.CreateBlogPostAsync(topic, audience, writingStyle, researchArtifacts, cancellationToken).ConfigureAwait(false);
    }
}

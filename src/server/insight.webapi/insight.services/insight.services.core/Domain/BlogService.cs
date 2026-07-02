using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

public class BlogService(IAgent agent) : IBlogService
{
    private readonly IAgent _agent = agent;

    public async Task<string> CreateBlogEntryAsync(string topic, CancellationToken cancellationToken = default)
    {
        // place for validation, persistence, metrics, etc.
        return await _agent.CreateBlogPostAsync(topic, cancellationToken);
    }
}

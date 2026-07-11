using Insight.Services.Interfaces.Core;

namespace Insight.Services.Interfaces.Ai;

public interface IBlogAgent
{
    Task<BlogEntry> CreateBlogPostAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);
}

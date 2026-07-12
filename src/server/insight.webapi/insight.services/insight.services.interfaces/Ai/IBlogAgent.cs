using Insight.Services.Interfaces.Ai.Events;
using Insight.Services.Interfaces.Core;

namespace Insight.Services.Interfaces.Ai;

public interface IBlogAgent
{
    Task<BlogEntry> CreateBlogPostAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);
    Task<BlogEntry> CreateBlogPostStreamedAsync(string topic, string audience, string writingStyle, IEventBus eventBus, CancellationToken cancellationToken = default);
}

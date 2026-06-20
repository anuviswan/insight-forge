namespace Insight.Services.Interfaces.Ai;

public interface IAgent
{
    Task<string> CreateBlogPostAsync(string topic, CancellationToken cancellationToken = default);
}

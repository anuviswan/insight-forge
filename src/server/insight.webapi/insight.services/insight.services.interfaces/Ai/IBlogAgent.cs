namespace Insight.Services.Interfaces.Ai;

public interface IBlogAgent
{
    Task<string> CreateBlogPostAsync(string topic, string audience, string writingStyle, string researchArtifacts, CancellationToken cancellationToken = default);
}

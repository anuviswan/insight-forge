using System.Threading;
using System.Threading.Tasks;

namespace Insight.Services.Interfaces.Core;

public interface IBlogService
{
    Task<string> CreateBlogEntryAsync(string topic, string audience, string writingStyle, string researchArtifacts, CancellationToken cancellationToken = default);
}

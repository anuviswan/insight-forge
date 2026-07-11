using System.Threading;
using System.Threading.Tasks;

namespace Insight.Services.Interfaces.Core;

public interface IBlogService
{
    Task<BlogEntry> CreateBlogEntryAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);
}

public class BlogEntry
{
    public string Content { get; set; } = string.Empty;
    public List<string> Citations { get; set; } = new();
    public List<string> References { get; set; } = new();
    public ContentQualityAssessment? QualityAssessment { get; set; }
}

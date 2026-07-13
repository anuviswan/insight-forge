using System.Threading;
using System.Threading.Tasks;

namespace Insight.Services.Interfaces.Core;

public interface IBlogService
{
    Task<BlogEntry> CreateBlogEntryAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start blog generation as a background job with streaming status events.
    /// </summary>
    /// <returns>Job identifier used to subscribe to status events and retrieve the result</returns>
    Task<string> StartBlogEntryJobAsync(string topic, string audience, string writingStyle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the result of a previously started blog generation job.
    /// </summary>
    BlogJobResult? GetJobResult(string jobId);
}

public class BlogEntry
{
    public string Content { get; set; } = string.Empty;
    public List<string> Citations { get; set; } = new();
    public List<string> References { get; set; } = new();
    public ContentQualityAssessment? QualityAssessment { get; set; }
}

/// <summary>
/// Outcome of an asynchronous blog generation job.
/// </summary>
public class BlogJobResult
{
    public BlogEntry? Entry { get; set; }
    public string? Error { get; set; }
    public bool IsSuccess => Error == null && Entry != null;
}

/// <summary>
/// Stores results of asynchronous blog generation jobs, keyed by job identifier.
/// </summary>
public interface IBlogJobResultStore
{
    void SetResult(string jobId, BlogEntry entry);
    void SetError(string jobId, string error);
    BlogJobResult? GetResult(string jobId);
}

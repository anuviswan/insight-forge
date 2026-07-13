using System.Collections.Concurrent;
using Insight.Services.Interfaces.Core;

namespace Insight.WebApi.Services;

/// <summary>
/// Thread-safe in-memory store for asynchronous blog generation job results.
/// Registered as a singleton so results survive across the scoped request that created them.
/// </summary>
public class BlogJobResultStore : IBlogJobResultStore
{
    private readonly ConcurrentDictionary<string, BlogJobResult> _results = new();

    public void SetResult(string jobId, BlogEntry entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));

        _results[jobId] = new BlogJobResult { Entry = entry };
    }

    public void SetError(string jobId, string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));
        ArgumentException.ThrowIfNullOrWhiteSpace(error, nameof(error));

        _results[jobId] = new BlogJobResult { Error = error };
    }

    public BlogJobResult? GetResult(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        return _results.TryGetValue(jobId, out var result) ? result : null;
    }
}

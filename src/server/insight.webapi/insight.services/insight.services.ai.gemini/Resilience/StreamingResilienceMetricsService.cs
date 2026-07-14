using System.Collections.Concurrent;
using Insight.Services.Interfaces.Ai;

namespace Insight.Services.Ai.Gemini.Resilience;

/// <summary>
/// In-memory, process-lifetime implementation of <see cref="IStreamingResilienceMetrics"/>.
/// </summary>
public class StreamingResilienceMetricsService : IStreamingResilienceMetrics
{
    private readonly ConcurrentDictionary<string, long> _errorsByType = new();
    private long _totalRetryAttempts;
    private long _totalRetrySuccesses;
    private readonly IJobAgentService _jobAgentService;

    public StreamingResilienceMetricsService(IJobAgentService jobAgentService)
    {
        _jobAgentService = jobAgentService;
    }

    public void RecordError(string operation, string errorType)
    {
        var key = $"{operation}:{errorType}";
        _errorsByType.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public void RecordRetryAttempt(string operation)
    {
        Interlocked.Increment(ref _totalRetryAttempts);
    }

    public void RecordRetrySuccess(string operation)
    {
        Interlocked.Increment(ref _totalRetrySuccesses);
    }

    public ResilienceMetricsSnapshot GetSnapshot()
    {
        var queueDepths = new Dictionary<string, int?>();
        foreach (var jobId in _jobAgentService.GetActiveJobs())
        {
            queueDepths[jobId] = _jobAgentService.GetEventBus(jobId)?.QueueDepth;
        }

        return new ResilienceMetricsSnapshot
        {
            ErrorsByType = new Dictionary<string, long>(_errorsByType),
            TotalRetryAttempts = Interlocked.Read(ref _totalRetryAttempts),
            TotalRetrySuccesses = Interlocked.Read(ref _totalRetrySuccesses),
            ActiveJobQueueDepths = queueDepths
        };
    }
}

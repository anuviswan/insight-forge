namespace Insight.Services.Interfaces.Ai;

/// <summary>
/// Tracks error rates and retry outcomes for streaming agent operations, for
/// diagnostics and monitoring.
/// </summary>
public interface IStreamingResilienceMetrics
{
    /// <summary>
    /// Record that an operation failed with a given error classification (e.g. status code or exception type).
    /// </summary>
    void RecordError(string operation, string errorType);

    /// <summary>
    /// Record that a retry was attempted for an operation.
    /// </summary>
    void RecordRetryAttempt(string operation);

    /// <summary>
    /// Record that a retried operation eventually succeeded.
    /// </summary>
    void RecordRetrySuccess(string operation);

    /// <summary>
    /// Get a point-in-time snapshot of tracked metrics.
    /// </summary>
    ResilienceMetricsSnapshot GetSnapshot();
}

/// <summary>
/// Point-in-time snapshot of streaming resilience metrics.
/// </summary>
public class ResilienceMetricsSnapshot
{
    public Dictionary<string, long> ErrorsByType { get; set; } = new();
    public long TotalRetryAttempts { get; set; }
    public long TotalRetrySuccesses { get; set; }
    public double RetrySuccessRate => TotalRetryAttempts == 0 ? 0 : (double)TotalRetrySuccesses / TotalRetryAttempts;
    public Dictionary<string, int?> ActiveJobQueueDepths { get; set; } = new();
}

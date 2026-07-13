namespace Insight.Services.Ai.Gemini.Resilience;

/// <summary>
/// Executes an operation with automatic retry on transient failure, using the
/// backoff and retry-count limits configured via <see cref="Options.StreamingErrorPolicyOptions"/>.
/// </summary>
public interface IGeminiRetryPolicy
{
    /// <summary>
    /// Runs <paramref name="operation"/>, retrying on transient failures up to the configured limit.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">A short name identifying the operation, used for logging and metrics.</param>
    /// <param name="cancellationToken">Cancellation token. If already cancelled when a failure occurs, the failure is not retried.</param>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken cancellationToken = default);
}

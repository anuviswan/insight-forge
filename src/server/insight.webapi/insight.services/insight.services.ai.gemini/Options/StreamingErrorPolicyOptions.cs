namespace Insight.Services.Ai.Gemini.Options;

/// <summary>
/// Configures retry, timeout, and resilience behavior for Gemini agent streaming operations.
/// </summary>
public class StreamingErrorPolicyOptions
{
    /// <summary>
    /// Maximum number of retry attempts for a transient failure before giving up.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay before the first retry attempt. Subsequent delays grow by <see cref="RetryBackoffMultiplier"/>.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Multiplier applied to the retry delay after each failed attempt.
    /// </summary>
    public double RetryBackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Overall wall-clock budget for a single streamed interaction, including all retries.
    /// </summary>
    public TimeSpan StreamTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

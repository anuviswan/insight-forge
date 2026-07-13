using Insight.Services.Ai.Gemini.Options;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Insight.Services.Ai.Gemini.Resilience;

/// <summary>
/// Default <see cref="IGeminiRetryPolicy"/> implementation: retries transient HTTP
/// and I/O failures with exponential backoff, up to <see cref="StreamingErrorPolicyOptions.MaxRetries"/> attempts.
/// </summary>
public class GeminiRetryPolicy : IGeminiRetryPolicy
{
    private readonly StreamingErrorPolicyOptions _options;
    private readonly IStreamingResilienceMetrics _metrics;
    private readonly ILogger<GeminiRetryPolicy> _logger;

    public GeminiRetryPolicy(
        IOptions<StreamingErrorPolicyOptions> options,
        IStreamingResilienceMetrics metrics,
        ILogger<GeminiRetryPolicy> logger)
    {
        _options = options.Value;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var attempt = 0;
        var delay = _options.InitialRetryDelay;

        while (true)
        {
            try
            {
                var result = await operation().ConfigureAwait(false);
                if (attempt > 0)
                {
                    _metrics.RecordRetrySuccess(operationName);
                    _logger.LogInformation("{Operation} succeeded after {Attempt} retries", operationName, attempt);
                }
                return result;
            }
            catch (Exception ex) when (attempt < _options.MaxRetries && TransientErrorClassifier.IsRetryable(ex, cancellationToken))
            {
                attempt++;
                _metrics.RecordError(operationName, TransientErrorClassifier.Classify(ex));
                _metrics.RecordRetryAttempt(operationName);
                _logger.LogWarning(
                    ex,
                    "{Operation} failed on attempt {Attempt}/{MaxRetries}, retrying in {Delay}",
                    operationName, attempt, _options.MaxRetries, delay);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _options.RetryBackoffMultiplier);
            }
            catch (Exception ex)
            {
                _metrics.RecordError(operationName, TransientErrorClassifier.Classify(ex));
                throw;
            }
        }
    }
}

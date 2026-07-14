using System.Net;

namespace Insight.Services.Ai.Gemini.Resilience;

/// <summary>
/// Classifies exceptions from Gemini API calls as transient (worth retrying) or not,
/// shared by both the request-level retry policy and the stream-resume retry loop.
/// </summary>
internal static class TransientErrorClassifier
{
    // 4xx codes are generally not retryable (the request itself is invalid), except
    // 408 (request timeout) and 429 (rate limited), which are transient in practice.
    private static readonly HashSet<HttpStatusCode> RetryableStatusCodes =
    [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    ];

    public static bool IsRetryable(Exception ex, CancellationToken cancellationToken)
    {
        // If our own caller asked us to stop, this is a deliberate cancellation, not a transient failure.
        if (cancellationToken.IsCancellationRequested)
            return false;

        return ex switch
        {
            HttpRequestException httpEx => httpEx.StatusCode is null || RetryableStatusCodes.Contains(httpEx.StatusCode.Value),
            TaskCanceledException or TimeoutException => true,
            IOException => true,
            _ => false
        };
    }

    public static string Classify(Exception ex) => ex switch
    {
        HttpRequestException httpEx => httpEx.StatusCode?.ToString() ?? "HttpRequestException",
        TaskCanceledException or TimeoutException => "Timeout",
        IOException => "IOException",
        _ => ex.GetType().Name
    };
}

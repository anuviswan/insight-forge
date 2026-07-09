namespace Insight.Services.Interfaces.Core;

/// <summary>
/// Service for managing login attempts and rate limiting.
/// </summary>
public interface ILoginAttemptService
{
    /// <summary>
    /// Record a login attempt (successful or failed).
    /// </summary>
    Task RecordAttemptAsync(string email, string? userId, bool isSuccessful, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if account is currently locked due to failed attempts.
    /// </summary>
    Task<bool> IsAccountLockedAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get time remaining until lockout expires (null if not locked).
    /// </summary>
    Task<TimeSpan?> GetLockoutTimeRemainingAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment failed login attempts and lock account if threshold exceeded.
    /// </summary>
    Task IncrementFailedAttemptsAsync(string email, CancellationToken cancellationToken = default);
}
